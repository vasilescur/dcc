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

namespace DCC {
    class Emitter {
        AbstractProgram program;

        public Emitter(AbstractProgram program) {
            this.program = program;
        }

        ///<summary>Generates a full assembly language program from the given <c>AbstractProgram</c>.
        ///</summary>
        public List<string> EmitAssembly() {
            List<string> fullAsm = new List<string>();

            fullAsm.AddRange(this.EmitTextSection());
            fullAsm.AddRange(this.EmitDataSection());

            return fullAsm;
        }

        ///<summary>Generates the <c>.data</c> section of an assembly language program.
        ///</summary>
        private List<string> EmitDataSection() {
            List<string> result = new List<string>();
            result.Add(".data");

            // Emit the initial stack pointer value
            result.Add("__sp_init: .word 32767");

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

        ///<summary>Generates the <c>.text</c> section of an assembly language program.
        ///</summary>
        private List<string> EmitTextSection() {
            List<string> result = new List<string>();
            result.Add(".text");

            // First, we need to set up the stack.
            // This stack starts at 0x7FFF = 32767
            // This is too big to do an addi, so we have to load it from memory
            //result.Add("    addi    $r6, $r0, 32767");
            result.Add("    la      $r1,    __sp_init");
            result.Add("    lw      $r6,    0($r1)");
            result.Add("    xor     $r1,    $r1,    $r1");

            // Our entry point will be the function _main, so the first real instruction
            // should be a jump to that function.
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

                result.Add("");

                // Emit actual code for instructions.
                foreach (Action action in function.actions) {
                    if (action is InlineAssembly) {
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
                                ref stackMap
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
                                name = "__temp_expr", 
                                type = Variable.VarType.Int
                            };

                            result.Add("    addi    $r6,    $r6,    -1");
                            stackMap.Add(tempResult, 0);

                            List<string> steps = EvaluateExpression(
                                (action as FunctionCall).arguments[argNum - 1],
                                tempResult,
                                ref stackMap
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
                    }
                }
            }

            return result;
        }

        private List<string> EvaluateExpression(Expression expression, 
                                                Variable dest, 
                                                ref Dictionary<Variable, int> stackMap) {
            List<string> result = new List<string>();

            if (expression is LiteralConstant) {
                if (stackMap.ContainsKey(dest)) {
                    // Destination is already on the stack.
                    int destOffset = stackMap[dest];

                    // Need to back up $r1, set $r1 to the value, then 
                    // sw $r1 --> stack[offset], then restore $r1

                    result.Add("    addi    $r6,    $r6,    -1");       // Expand the stack
                    result.Add("    sw      $r1,    0($r6)");           // Backup $r1 to stack
                    result.Add("    xor     $r1,    $r1,    $r1");      // Zero out $r1

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

                    result.Add(
                        "    sw      $r1,    " + (destOffset + 1).ToString()    // Write to stack[dest]
                        + "($r6)"   // Offset of destOffset + 1 because we backed up $r1 to stack
                    );
                    result.Add("    lw      $r1,    0($r6)");           // Restore $r1 from stack
                    result.Add("    addi    $r6,    $r6,    1");       // Restore the stack

                    return result;
                } else {
                    // Destination doesn't exist on the stack.
                    throw new ArgumentOutOfRangeException("Destination: " + dest.ToString() + " not on stack.");
                }
            } else {
                throw new NotImplementedException("Only supports evaluation of literal constants.");
            }
        }

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