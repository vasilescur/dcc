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

        public List<string> EmitAssembly() {
            List<string> fullAsm = new List<string>();

            fullAsm.AddRange(this.EmitTextSection());
            fullAsm.AddRange(this.EmitDataSection());

            return fullAsm;
        }

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

        private List<string> EmitTextSection() {
            List<string> result = new List<string>();
            result.Add(".text");



            return result;
        }
    }
}