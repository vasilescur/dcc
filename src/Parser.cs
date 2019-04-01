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

        private List<Variable> currentLocals;

        private Stack<List<Variable>> scope = new Stack<List<Variable>>();

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
                throw new UnexpectedTokenException(result, expectedType);
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

        public AbstractProgram Parse() {
            this.program = new AbstractProgram();

            // Process all items in the global scope
            while (Peek().type != Token.TokenType.EOF) {
                while (Peek().type == Token.TokenType.Semicolon) {
                    Consume(Token.TokenType.Semicolon);
                }

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
                    // typeName : Token - already consumed --> Return Value
                    Variable.VarType returnType;

                    switch (typeName.content) {
                        case "int": {
                            returnType = Variable.VarType.Int;
                        } break;

                        case "int*": {
                            returnType = Variable.VarType.IntPtr;
                        } break;

                        case "void": {
                            returnType = Variable.VarType.Void;
                        } break;

                        default: {
                            throw new InvalidTypeException(typeName.content);
                        }
                    }

                    string newFuncName = Consume(Token.TokenType.FuncName).content;

                    // Now, we need to read the function's arguments
                    List<Variable> newArgs = ParseFunctionSignature();

                    Consume(Token.TokenType.BraceOpen);

                    // Now, we need the actions/instructions contained in the function!
                    this.currentLocals = new List<Variable>();
                    List<Action> actions = ParseBlock();

                    this.program.functions.Add(new Function() {
                        name = newFuncName,
                        returnsValue = returnType,
                        arguments = newArgs,
                        actions = actions,
                        localVars = this.currentLocals
                    });

                } else {
                    throw new UnexpectedTokenException(Consume());
                }
            }

            return this.program;
        }

        private List<Variable> ParseFunctionSignature() {
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

            return newArgs;
        }

        private bool EndOfExpr() {
            return (new List<Token.TokenType> {
                Token.TokenType.ParenClose,
                Token.TokenType.Semicolon
            }.Contains(Peek().type));
        }

        private Expression ParseExpression() {
            // What's the first token?
            if (Peek().type == Token.TokenType.LiteralInt) {
                Expression firstLiteral = new LiteralConstant() {
                    value = int.Parse(Consume(Token.TokenType.LiteralInt).content)
                };

                var restOfExpr = RestOfExpr(firstLiteral);

                if (restOfExpr is null) {
                    return firstLiteral;
                } else {
                    return restOfExpr;
                }

            } else if (Peek().type == Token.TokenType.ParenOpen) {
                // Sub-expression
                Consume(Token.TokenType.ParenOpen);
                Expression subExpr = ParseExpression();
                Consume(Token.TokenType.ParenClose);

                var restOfExpr = RestOfExpr(subExpr);

                if (restOfExpr is null) {
                    return subExpr;
                } else {
                    return restOfExpr;
                }
            } else if (Peek().type == Token.TokenType.VarName) {
                Token varName = Consume(Token.TokenType.VarName);
                Variable var = this.currentLocals.Find(v => v.name == varName.content);

                var restOfExpr = RestOfExpr(var);

                if (restOfExpr is null) {
                    return var;
                } else {
                    return restOfExpr;
                }
            } else if (Peek().type == Token.TokenType.FuncName) {
                Token funcName = Consume(Token.TokenType.FuncName);
                Function func = program.functions.Find(f => f.name == funcName.content);

                List<Expression> ps = ParseFunctionArguments();

                return new FunctionCall() {
                    function = func,
                    arguments = ps
                };
            }

            else {
                throw new UnexpectedTokenException(Consume());
            }

        }

        private Expression RestOfExpr(Expression soFar) {
            if (Peek().type == Token.TokenType.Comma) {
                // Means we're parsing some function params. Done with this one!
                return soFar;
            }

            if (EndOfExpr()) {    // End of expression
                return null;    // The expression was just an int literal
            }

            if (Token.IsExpressionOperator(Peek())) {   // +, -, ==, >>, etc
                Token opToken = Consume();

                Operation.OperationType opType = Operation.ParseType(opToken);

                bool twoParam = Operation.IsTwoParam(opType);

                if (!twoParam) {
                    //TODO: Allow prefix operators
                    // Currently, all ops must be infix or posfix.

                    SingleParamOperation operation = new SingleParamOperation() {
                        operationType = opType,
                        operand = soFar
                    };

                    return operation;
                } else {
                    // Two parameter.

                    // Parse the second parameter
                    Expression secondParam = ParseExpression();

                    return new TwoParamOperation() {
                        operationType = opType,
                        operands = new List<Expression>() {
                            soFar, secondParam
                        }
                    };
                }
            } else {
                // Well..... An Expression has to be (Expression) or (Expression OP) or (Expression OP Expression)
                // So if we're here... it's a syntax error I think
                System.Console.WriteLine("Error: Expected operator or end of expression. Found token");
                throw new UnexpectedTokenException(Consume());
            }

            // Can't ever get here
            throw new Exception("This is impossible");
        }

        private List<Expression> ParseFunctionArguments() {
            List<Expression> result = new List<Expression>();

            Consume(Token.TokenType.ParenOpen);

            while (Peek().type != Token.TokenType.ParenClose) {
                Expression currentParam = ParseExpression();
                result.Add(currentParam);

                if (Peek().type == Token.TokenType.Comma) {
                    Consume(Token.TokenType.Comma);
                }
            }
            
            Consume(Token.TokenType.ParenClose);

            return result;
        }

        private List<Action> ParseBlock() {
            List<Action> actions = new List<Action>();

            int nestLevel = 0;

            while (Peek().type != Token.TokenType.BraceClose) {
                if (Peek().type == Token.TokenType.Semicolon) {
                    Consume(Token.TokenType.Semicolon);
                    continue;
                }

                // Instructions
                if (Peek().type == Token.TokenType.FuncName) {  // Means we are calling a void function
                    string fnName = Consume(Token.TokenType.FuncName).content;
                    Function func = this.program.functions.Find(f => f.name == fnName);

                    List<Expression> arguments = ParseFunctionArguments();
                    Consume(Token.TokenType.Semicolon);
                    
                    actions.Add(new FunctionCall() {
                        function = func,
                        arguments = arguments 
                    });
                } else if (Peek().type == Token.TokenType.TypeName) {
                    // Declaring a local variable!
                    Token typeName = Consume(Token.TokenType.TypeName);

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

                    Variable newVar = new Variable {
                        name = newVarName, 
                        type = newVarType
                    };

                    currentLocals.Add(newVar);
                    actions.Add(new LocalVarDeclaration() {
                        variable = newVar,
                        initValue = initValue
                    });
                } else if (Peek().type == Token.TokenType.VarName) {
                    // Variable assignment!
                    Token targetName = Consume(Token.TokenType.VarName);

                    // What value?
                    Expression newVal = ParseExpression();

                    // Is it a global?
                    if (program.globalVars.Any(v => v.name == targetName.content)) {
                        actions.Add(new GlobalVarAssignment() {
                            variable = program.globalVars.Find(v => v.name == targetName.content),
                            newValue = newVal
                        });
                    } else {
                        actions.Add(new VarAssignment() {
                            variable = this.currentLocals.Find(v => v.name == targetName.content),
                            newValue = newVal
                        });
                    }
                } else if (Peek().type == Token.TokenType.Assembly) {
                    Token inlineAsm = Consume(Token.TokenType.Assembly);

                    actions.Add(new InlineAssembly() {
                        code = inlineAsm.content.Split(";").ToList()
                    });
                } else if (Peek().type == Token.TokenType.KwIf) {
                    Consume(Token.TokenType.KwIf);
                    Consume(Token.TokenType.ParenOpen);

                    Expression condition = ParseExpression();

                    List<Action> body = ParseBlock();

                    if (Peek().type == Token.TokenType.KwElse) {
                        Consume(Token.TokenType.BraceOpen);
                        List<Action> elseBody = ParseBlock();

                        actions.Add(new IfElse() {
                            condition = condition,
                            ifActions = body,
                            elseActions = elseBody
                        });
                    } else {
                        actions.Add(new If() {
                            condition = condition,
                            ifActions = body
                        });
                    }
                } else if (Peek().type == Token.TokenType.KwWhile) {
                    Consume(Token.TokenType.KwWhile);
                    Consume(Token.TokenType.ParenOpen);

                    Expression condition = ParseExpression();

                    List<Action> body = ParseBlock();

                    actions.Add(new While() {
                        condition = condition,
                        actions = body
                    });
                } //TODO: For
                //TODO: Goto

                else if (Peek().type == Token.TokenType.KwReturn) {
                    Consume(Token.TokenType.KwReturn);

                    if (Peek().type == Token.TokenType.Semicolon) {
                        Consume(Token.TokenType.Semicolon);
                        actions.Add(new ReturnInstruction() {
                            returnValue = null
                        });
                    } else {
                        Expression returnValue = ParseExpression();
                        actions.Add(new ReturnInstruction() {
                            returnValue = returnValue
                        });
                    }
                } else {
                    throw new UnexpectedTokenException(Consume());
                }
            }

            if (Peek().type == Token.TokenType.BraceClose) {
                Consume(Token.TokenType.BraceClose);
            }

            return actions;
        }

        [System.Serializable]
        public class UnexpectedTokenException : System.Exception
        {
            public UnexpectedTokenException() : base("Unexpected Token.") { 
                //Environment.Exit(1);
            }

            public UnexpectedTokenException(Token offender) : base("Unexpected Token: " + offender.ToString()) { 
                //Environment.Exit(1);
            }

            public UnexpectedTokenException(Token offender, Token.TokenType expected) : base(
                "Unexpected Token: " + offender.ToString() + " (expected " + expected.ToString() + ")"
            ) {
                //Environment.Exit(1);
            }
        }

        [System.Serializable]
        public class InvalidTypeException : System.Exception
        {
            public InvalidTypeException() : base("Invalid Type.") { 
                //Environment.Exit(1);
            }

            public InvalidTypeException(string offender) : base("Invalid Type: " + offender.ToString()) { 
                //Environment.Exit(1);
            }
        }
    }
};