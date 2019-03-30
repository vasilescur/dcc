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
        int position = 0;

        public Parser(List<Token> source) {
            this.source = source;
        }

        private Token Consume() {
            Token result = source[position];
            position++;
            return result;
        }

        private Token Consume(Token.TokenType expectedType) {
            Token result = Consume();
            if (result.type != expectedType) {
                throw new UnexpectedTokenException(result);
            } else {
                return result;
            }
        }

        private Token Peek() {
            return source[position];
        }

        private Token Prev() {
            if (position == 0) {
                throw new ArgumentOutOfRangeException();
            }

            return source[position - 1];
        }

        public void Parse() {
            // Process all items in the global scope
            while (Peek().type != Token.TokenType.EOF) {
                // For a global object, first token will always be the type
                Token typeName = Consume(Token.TokenType.TypeName);
                
                // Is it a function or a global variable?
                if (Peek().type == Token.TokenType.VarName) {
                    string newVarName = Consume(Token.TokenType.VarName).content;
                    Variable.VarType newVarType;

                    switch (typeName.content) {
                        case "int": {
                            newVarType = Variable.VarType.Int;
                        } break;

                        case "int*": {
                            newVarType = Variable.VarType.IntPtr;
                        } break;

                        default: {
                            throw new InvalidTypeException(typeName.content);
                        }
                    }

                    int initValue;

                    // Are we initializing with a value?
                    if (Peek().type == Token.TokenType.OpAssign) {
                        Consume(Token.TokenType.OpAssign);

                        // Is it an int or a char?
                        if (Peek().type == Token.TokenType.LiteralChar) {
                            initValue = Consume(Token.TokenType.LiteralChar).content[0];
                        } else {
                            initValue = int.Parse(Consume(Token.TokenType.LiteralInt).content);
                        }
                    } else {
                        initValue = 0;  // Default value is 0
                    }

                    GlobalVariable newVar = new GlobalVariable {
                        name = newVarName, 
                        type = newVarType,
                        initValue = initValue
                    };

                    program.globalVars.Add(newVar);

                } else if (Peek().type == Token.TokenType.FuncName) {
                    // typeName : Token - already consumed

                    string newFuncName = Consume(Token.TokenType.FuncName).content;

                    // Now, we need to read the function's arguments
                    Consume(Token.TokenType.ParenOpen);

                    List<Variable> newArgs = new List<Variable>();

                    while (Peek().type == Token.TokenType.TypeName) {
                        string argTypeName = Consume(Token.TokenType.TypeName).content;
                        Variable.VarType argType;

                        switch (argTypeName) {
                            case "int": {
                                argType = Variable.VarType.Int;
                            } break;

                            case "int*": {
                                argType = Variable.VarType.IntPtr;
                            } break;

                            default: {
                                throw new InvalidTypeException(argTypeName);
                            }
                        }

                        string argName = Consume(Token.TokenType.VarName).content;

                        newArgs.Add(new Variable() {
                            name = argName,
                            type = argType
                        });

                        // Set ourselves up for the next type name, if there is one
                        if (Peek().type == Token.TokenType.Comma) {
                            Consume(Token.TokenType.Comma);
                        }
                    }

                    Consume(Token.TokenType.ParenClose);
                    Consume(Token.TokenType.BraceOpen);

                    // Now, we need the actions/instructions contained in the function!



                } else {
                    throw new UnexpectedTokenException(Consume());
                }
            }
        }

        [System.Serializable]
        public class UnexpectedTokenException : System.Exception
        {
            public UnexpectedTokenException() : base("Unexpected Token.") { 
                Environment.Exit(1);
            }

            public UnexpectedTokenException(Token offender) : base("Unexpected Token: " + offender.ToString()) { 
                Environment.Exit(1);
            }
        }

        [System.Serializable]
        public class InvalidTypeException : System.Exception
        {
            public InvalidTypeException() : base("Invalid Type.") { 
                Environment.Exit(1);
            }

            public InvalidTypeException(string offender) : base("Invalid Type: " + offender.ToString()) { 
                Environment.Exit(1);
            }
        }
    }
}