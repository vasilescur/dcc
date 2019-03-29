///<summary>
///The parser transformes <c>Token</c>ized code into an Abstract Syntax Tree format, grouping
///together the tokens into distinct instructions with nested components.
///</summary>
///<author>Radu Vasilecu</author>
///<date>2019-03-29</date>

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCC {
    class Parser {
        public AbstractProgram program;
        public List<Token> source;

        public Parser(List<Token> source) {
            this.source = source;
        }

        public void Parse() {
            string context = "global";
            string subcontext = "";
            int exprLevel = 0;

            string temp_type = "";
            string temp_name = "";

            foreach (Token token in source) {
                switch (context) {
                    case "global": {
                        // First token *has* to be a type name
                        
                    } break;

                    case "function": {
                        switch (subcontext) {
                            case "args": {

                            } break;

                            case "": {      // General function instructions

                            } break;

                            default: {
                                throw new Exception("Unknown Context: " + context + "/" + subcontext);
                            }
                        }
                    } break;

                    default: {
                        throw new Exception("Unknown Context: " + context);
                    }
                }
            }
        }
    }
}