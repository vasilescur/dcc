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

            foreach (Function func in program.functions) {
                System.Console.WriteLine("\nFunction: " + func.name);

                foreach (Action act in func.actions) {
                    System.Console.WriteLine("  Action: " + act.ToString());
                }
            }

            return result;
        }
    }
}