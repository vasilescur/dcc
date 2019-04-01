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
            result.Add("    addi    $r6, $r0, 32767");

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
                    }
                }

                // Restore registers from the stack

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
            }

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