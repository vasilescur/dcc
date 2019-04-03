///<summary>
///The Emitter takes the <c>AbstractProgran</c> representation and uses it to emit/generate
///the corresponding assembly language instructions for the Duke 250/16 architecture.
///</summary>
///<author>Radu Vasilecu</author>
///<date>2019-03-30</date>

using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace dcc {
    ///<summary>
    /// Allows a program to generate many unique identifiers.
    /// This is useful for the compiler because we often need to create a uniquely named
    /// label or variable name that will only be used in a specific scope or action.
    ///</summary>
    static class UniqueIdentifier {
        private static int currentValue = 0;

        public static int Next() {
            int result = currentValue;
            currentValue++;
            return result;
        }

        public static string NextStr() {
            return Next().ToString();
        }
    }

    class Emitter {
        AbstractProgram program;

        public Emitter(AbstractProgram program) {
            this.program = program;
        }

        ///<summary>
        /// Generates a full assembly language program from the given <c>AbstractProgram</c>.
        ///</summary>
        public List<string> EmitAssembly() {
            List<string> fullAsm = new List<string>();

            fullAsm.AddRange(this.EmitTextSection());
            fullAsm.AddRange(this.EmitDataSection());

            return fullAsm;
        }

        ///<summary>
        /// Generates the <c>.data</c> section of an assembly language program.
        ///</summary>
        private List<string> EmitDataSection() {
            List<string> result = new List<string>();
            result.Add(".data");

            // Emit the initial stack pointer value
            result.Add("# Initial stack pointer value");
            result.Add("__sp_init: .word 32767");
            result.Add("");

            result.Add("# Global Variables: ");
            foreach (GlobalVariable variable in program.globalVars) {
                StringBuilder currentLine = new StringBuilder("");

                currentLine.Append(variable.name);
                currentLine.Append(": ");

                switch(variable.type) {
                    case Variable.VarType.Int:
                    case Variable.VarType.IntPtr: {
                        currentLine.Append(".word ");
                    } break;

                    default: {
                        throw new Parser.InvalidTypeException(variable.type.ToString());
                    }
                }

                currentLine.Append(variable.initValue.ToString());

                result.Add(currentLine.ToString());
            }

            return result;
        }

        ///<summary>
        /// Generates the <c>.text</c> section of an assembly language program.
        ///</summary>
        private List<string> EmitTextSection() {
            List<string> result = new List<string>();
            result.Add(".text");

            // First, we need to set up the stack.
            // This stack starts at 0x7FFF = 32767
            // This is too big to do an addi, so we have to load it from memory
            //result.Add("    addi    $r6, $r0, 32767");
            result.Add("    # Set up the stack pointer ");
            result.Add("    la      $r1,    __sp_init");
            result.Add("    lw      $r6,    0($r1)");
            result.Add("    xor     $r1,    $r1,    $r1");

            // Our entry point will be the function _main, so the first real instruction
            // should be a jump to that function.
            result.Add("\n    # Jump to the entry point");
            result.Add("    jal     main");

            // When we return back here, need to halt!
            result.Add("    halt");

            result.Add("");

            // Now, we have to emit the code for each function in turn.
            foreach(Function function in program.functions) {
                // Function header
                string header = FunctionHeader(function);
                result.Add(header);

                result.Add(function.name + ":");

                // How much stack space will we need?
                // 1 for each register (including ra), $r1 - $r4, $r7
                //  --> 5
                // 1 for each local variable

                int stackSize = 0;
                stackSize += 5;
                stackSize += function.localVars.Count;

                result.Add("    # Allocate stack space and back up registers");
                result.Add("    addi    $r6,    $r6,    -" + stackSize.ToString());

                // Store the registers on the stack
                result.Add("    sw      $r1,    0($r6)");
                result.Add("    sw      $r2,    1($r6)");
                result.Add("    sw      $r3,    2($r6)");
                result.Add("    sw      $r4,    3($r6)");
                // Register 5 is for the return address, therefore don't save. Zero it out!
                result.Add("    xor     $r5,    $r5,    $r5");
                result.Add("    sw      $r7,    4($r6)");

                // Holds positive (upwards) offsets from the stack pointer
                Dictionary<Variable, int> stackMap = new Dictionary<Variable, int>();

                int i = 5;  // start off at +5 (after/above registers)

                foreach (Variable variable in function.localVars) {
                    stackMap.Add(variable, i);

                    // These don't have to go on the stack yet, they will be placed there
                    // as they are used and assigned to. Just have to zero it out...
                    result.Add("    sw      $r0,    " + i + "($r6)");

                    i++;
                }

                // Emit actual code for instructions.
                foreach (Action action in function.actions) {
                    EmitActionCode(ref result, action, function, ref stackMap, ref stackSize);
                }
            }

            return result;
        }

        private void EmitActionCode(ref List<string> result,
                                    Action action,
                                    Function function,
                                    ref Dictionary<Variable, int> stackMap,
                                    ref int stackSize) {
            // Emit comment with action ToString header
            result.Add("\n    # " + action.ToString());

            if (action is InlineAssembly) {
                // For an inline assembly command, we just need to output all of the lines
                // directly to the .text section
                foreach (string line in (action as InlineAssembly).code) {
                    result.Add("    " + line);
                }
            } else if (action is ReturnInstruction) {
                if (!((action as ReturnInstruction).returnValue is null)) {
                    // Need to evaluate the returnValue and place it in $r5
                    // $r5 isn't on the stack, so we have to define a new temp variable
                    Variable tempResult = new Variable() {
                        name = "__temp_expr",
                        type = Variable.VarType.Int
                    };

                    result.Add("    addi    $r6,    $r6,    -1");
                    stackMap.Add(tempResult, 0);

                    List<string> steps = EvaluateExpression(
                        (action as ReturnInstruction).returnValue,
                        tempResult,
                        ref stackMap,
                        1
                    );

                    result.AddRange(steps);

                    // Result is now in tempResult. Need to put it in $r5 and then jr $ra

                    result.Add("    xor     $r5,    $r5,    $r5");
                    result.Add("    lw      $r5,    " + stackMap[tempResult] + "($r6)");

                    // Restore the dirty stack
                    stackMap.Remove(tempResult);
                    result.Add("    addi    $r6,    $r6,    1");
                }

                result.AddRange(GenerateReturn(stackSize));
            } else if (action is FunctionCall) {
                // First, we need to evaluate each argument onto the stack
                // How many args?
                int numArgs = (action as FunctionCall).arguments.Count;

                if (numArgs > 4) {
                    throw new ArgumentException(
                        "Can't have more than 4 arguments (" + (action as FunctionCall).function.name + ")"
                    );
                }

                // Evaluate all arguments -->
                for (int argNum = 1; argNum <= numArgs; argNum++) {
                    Variable tempResult = new Variable() {
                        name = "__temp_expr_" + UniqueIdentifier.NextStr(),
                        type = Variable.VarType.Int
                    };

                    result.Add("    addi    $r6,    $r6,    -1");
                    stackMap.Add(tempResult, 0);

                    List<string> steps = EvaluateExpression(
                        (action as FunctionCall).arguments[argNum - 1],
                        tempResult,
                        ref stackMap,
                        1
                    );

                    result.AddRange(steps);

                    // Result is now in tempResult. Need to put it in correct arg register

                    result.Add(
                        "    lw      $r" + argNum.ToString() + ",    " + stackMap[tempResult] + "($r6)"
                    );

                    // Restore the dirty stack
                    stackMap.Remove(tempResult);
                    result.Add("    addi    $r6,    $r6,    1");
                }

                // Now, do the function call
                string calleeName = (action as FunctionCall).function.name;

                result.Add("    jal     " + calleeName);
            } else if (action is LocalVarDeclaration) {
                // The variable already exists!
                // Only need to do something if we are initializing it with a value.
                Variable target = (action as LocalVarDeclaration).variable;
                Expression newVal = (action as LocalVarDeclaration).initValue;

                // Need to evaluate the new value and place it in the target

                List<string> steps = EvaluateExpression(
                    newVal,
                    target,
                    ref stackMap,
                    1
                );

                result.AddRange(steps);
            } else if (action is VarAssignment) {
                // The variable already exists!
                // Only need to do something if we are initializing it with a value.
                Variable target = (action as VarAssignment).variable;
                Expression newVal = (action as VarAssignment).newValue;

                // Need to evaluate the new value and place it in the target

                List<string> steps = EvaluateExpression(
                    newVal,
                    target,
                    ref stackMap,
                    0
                );

                result.AddRange(steps);
            } else if (action is If) {
                // First, need to evaluate the if condition
                Expression ifCond = (action as If).condition;

                Variable tempResult = new Variable() {
                    name = "__temp_expr_" + UniqueIdentifier.NextStr(),
                    type = Variable.VarType.Int
                };

                result.Add("    addi    $r6,    $r6,    -2");
                stackMap.Add(tempResult, 0);

                List<string> steps = EvaluateExpression(
                    ifCond,
                    tempResult,
                    ref stackMap,
                    2
                );

                result.AddRange(steps);

                // Result is now in tempResult

                //result.Add("    sw      $r1,    1($r6)");
                result.Add("    lw      $r1,    " + (stackMap[tempResult]+1) + "($r6)");

                // Result of condition is now in $r1
                // Can be either 1 or 0. If it's 1 --> keep going, but if it's 0 --> skip body.
                string skipLabel = "__if_skip_" + UniqueIdentifier.NextStr();

                result.Add("    addi    $r6,    $r6,    1");
                result.Add("    beq     $r1,    $r0,    " + skipLabel); // If cond = 0 --> skip
                //result.Add("    lw      $r1,    1($r6)");

                // Body of the IF block
                foreach (Action ifAction in (action as If).ifActions) {
                    EmitActionCode(ref result, ifAction, function, ref stackMap, ref stackSize);
                }

                result.Add("  " + skipLabel + ":");

                // Restore the dirty stack
                stackMap.Remove(tempResult);
                result.Add("    addi    $r6,    $r6,    1");
            }
        }

        ///<summary>
        /// Returns a list of assembly instructions that evaluate a given expression,
        /// returning the result in a specified <c>dest</c>ination <see>Variable</see>.
        ///</summary>
        private List<string> EvaluateExpression(Expression expression,
                                                Variable dest,
                                                ref Dictionary<Variable, int> stackMap,
                                                int stackClobber) {
            /*//TODO: Refactor `stackClobber`
                We need a better way of handling ad-hoc expands and reductions of the current
                stack frame. Currently, I use the `stackClobber` variable inconsistently in order
                to signify that there's a manually applied change from the `stackMap`, and to let
                this method know that when reading or writing to stack addresses that aren't a part
                of this evaluation, we need to offset by the `stackClobber`. However, this is dirty
                and hacky and inconsistent. 

                //* A much better approach would be to maintain the `stackMap` as an absolute invariant,
                extending its `.Add()` and `.Remove()` methods to update the stack positions of all the
                other variables. This would eliminate a lot of inconsistencies, hacky patch jobs, and 
                allow for much easier further expansion to more complicated operations involving the stack.
            */

            List<string> result = new List<string>();

            result.Add("    # [EvaluateExpression " + expression.ToString() + "]");

            if (stackMap.ContainsKey(dest)) {
                // Destination is already on the stack.
                int destOffset = stackMap[dest];

                // Need to back up $r1, set $r1 to the value, then
                // sw $r1 --> stack[offset], then restore $r1

                result.Add("    addi    $r6,    $r6,    -1");       // Expand the stack
                result.Add("    sw      $r1,    0($r6)");           // Backup $r1 to stack
                result.Add("    xor     $r1,    $r1,    $r1");      // Zero out $r1

                if (expression is LiteralConstant) {
                    int newVal = (expression as LiteralConstant).value;
                    while (newVal > 31) {
                        result.Add(
                            "    addi    $r1,    $r1,    31"
                        );

                        newVal -= 31;
                    }

                    result.Add(
                        "    addi    $r1,    $r1,    "
                        + newVal.ToString()  // Set $r1 = literal value
                    );
                } else if (expression is Variable) {
                    // This is a simple one. Simply get the variable's value and put it in $r1
                    int sourceOffset = stackMap[(expression as Variable)];

                    result.Add("    lw      $r1,    " + (sourceOffset + stackClobber + 1) + "($r6)");
                } else if (expression is SingleParamOperation) {
                    //TODO: Single parameter operators.

                    throw new NotImplementedException("Single param operators are not yet supported.");
                } else if (expression is TwoParamOperation) {
                    // Get an ID
                    string unique = UniqueIdentifier.NextStr();

                    // Evaluate the operands
                    Variable leftOperand = new Variable() {
                        name = "__temp_leftop_" + unique, 
                        type = Variable.VarType.Int
                    };

                    Variable rightOperand = new Variable() {
                        name = "__temp_rightop_" + unique,
                        type = Variable.VarType.Int
                    };

                    result.Add("    addi    $r6,    $r6,    -2");
                    stackMap.Add(leftOperand, 0);
                    stackMap.Add(rightOperand, 1);

                    result.AddRange(EvaluateExpression(
                        (expression as TwoParamOperation).operands[0],
                        leftOperand,
                        ref stackMap,
                        stackClobber + 2
                    ));

                    result.AddRange(EvaluateExpression(
                        (expression as TwoParamOperation).operands[1],
                        rightOperand,
                        ref stackMap,
                        stackClobber + 2
                    ));

                    result.Add("    # " + (expression as TwoParamOperation).ToString());

                    switch ((expression as TwoParamOperation).operationType) {
                        case Operation.OperationType.TestEq: {
                            // Load both evaluated operands into registers
                            result.Add("    lw      $r2,    " + (stackMap[leftOperand]) + "($r6)");
                            result.Add("    lw      $r3,    " + (stackMap[rightOperand]) + "($r6)");

                            // Get a unique ID
                            string ifUnique = UniqueIdentifier.NextStr();

                            // Assume they're equal
                            result.Add("    addi    $r1,    $r0,    1");

                            // If they are, done!
                            result.Add("    beq     $r2,    $r3,    __beq_" + ifUnique);

                            // Otherwise, $r1 <- 0
                            result.Add("    addi    $r1,    $r0,    0");

                            // Done
                            result.Add("  __beq_" + ifUnique + ":");
                        } break;

                        //TODO: Other comparisons
                        // case Operation.OperationType.TestGt: {

                        // } break;

                        // case Operation.OperationType.TestLt: {

                        // } break;

                        // case Operation.OperationType.TestGeq: {

                        // } break;

                        // case Operation.OperationType.TestLeq: {

                        // } break;

                        case Operation.OperationType.Addition: {
                            // Load both evaluated operands into registers
                            result.Add("    lw      $r2,    " + (stackMap[leftOperand]) + "($r6)");
                            result.Add("    lw      $r3,    " + (stackMap[rightOperand]) + "($r6)");

                            // Add them into another register
                            result.Add("    add     $r1,    $r2,    $r3");
                        } break;

                        case Operation.OperationType.Subtraction: {
                            // Load both evaluated operands into registers
                            result.Add("    lw      $r2,    " + (stackMap[leftOperand]) + "($r6)");
                            result.Add("    lw      $r3,    " + (stackMap[rightOperand]) + "($r6)");

                            // Subtract them into another register
                            result.Add("    sub     $r1,    $r2,    $r3");
                        } break;

                        case Operation.OperationType.Xor: {
                            // Load both evaluated operands into registers
                            result.Add("    lw      $r2,    " + (stackMap[leftOperand]) + "($r6)");
                            result.Add("    lw      $r3,    " + (stackMap[rightOperand]) + "($r6)");

                            // XOR them into another register
                            result.Add("    xor     $r1,    $r2,    $r3");
                        } break;

                        //TODO: Shift operators
                        // case Operation.OperationType.ShiftLeft: {

                        // } break;

                        // case Operation.OperationType.ShiftRight: {

                        // } break;

                        default: {
                            throw new InvalidOperationException(
                                "Invalid operation: "
                                + (expression as TwoParamOperation).operationType.ToString()
                            );
                        }
                    }

                    // Reset the stack
                    stackMap.Remove(leftOperand);
                    stackMap.Remove(rightOperand);
                    result.Add("    addi    $r6,    $r6,    2");
                } else {
                    throw new NotImplementedException("Only supports evaluation of literal constants.");
                }

                result.Add(
                    "    sw      $r1,    " + (destOffset + stackClobber).ToString()    // Write to stack[dest]
                    + "($r6)"   // Offset of destOffset + 1 because we backed up $r1 to stack
                );
                
                result.Add("    lw      $r1,    0($r6)");           // Restore $r1 from stack
                result.Add("    addi    $r6,    $r6,    1");       // Restore the stack

                return result;
            } else {
                // Destination doesn't exist on the stack.
                throw new ArgumentOutOfRangeException("Destination: " + dest.ToString() + " not on stack.");
            }
        }

        ///<summary>
        /// Generates what LLVM would call a "function epilogue", basically resetting the registers
        /// to the values we backed up onto the stack, discarding the stack frame, and returning.
        ///</summary>
        private List<string> GenerateReturn(int stackSize) {
            List<string> result = new List<string>();

            // Store the registers on the stack
            result.Add("    lw      $r1,    0($r6)");
            result.Add("    lw      $r2,    1($r6)");
            result.Add("    lw      $r3,    2($r6)");
            result.Add("    lw      $r4,    3($r6)");
            result.Add("    lw      $r7,    4($r6)");

            // Discard the stack frame
            result.Add("    addi    $r6,    $r6,    " + stackSize.ToString());

            // Return from the function
            result.Add("    jr      $r7");
            result.Add("");

            return result;
        }

        ///<summary>
        /// Outputs a one-liner comment containing the function signature, for readability and
        /// clarity in the outputted assembly.
        ///</summary>
        private static string FunctionHeader(Function function) {
            string header = "# " + function.name + " (";

            if (function.arguments.Count > 0) {
                for (int i = 0; i < function.arguments.Count; i++) {
                    header += function.arguments[i].type.ToString() + " "
                                + function.arguments[i].name;

                    if (i < function.arguments.Count - 1) {
                        header += ", ";
                    }
                }
            }

            header += ")";

            return header;
        }
    }
}