///<summary>An Abstract Syntax Tree is an abstract representation of a program using
///a tree, with nodes being semantic units of evaluation. This class contains a rich
///object-oriented schema for defining Abstract Syntax Trees.
///</summary>

using System;
using System.Collections;
using System.Collections.Generic;

namespace dcc {
    interface Expression { }

        abstract class Operation : Expression {
            public OperationType operationType;

            public enum OperationType {
                Addition, Subtraction,
                TestEq, TestGt, TestLt, TestGeq, TestLeq,
                Increment, Decrement,
                Not, Xor, ShiftLeft, ShiftRight,
                Dereference, AddressOf
            }

            public static OperationType ParseType(Token opToken) {
                var lookup = new Dictionary<Token.TokenType, OperationType> {
                    { Token.TokenType.OpPlus, OperationType.Addition },
                    { Token.TokenType.OpMinus, OperationType.Subtraction },
                    { Token.TokenType.OpTestEq, OperationType.TestEq },
                    { Token.TokenType.OpTestGreater, OperationType.TestGt },
                    { Token.TokenType.OpTestLess, OperationType.TestLt },
                    { Token.TokenType.OpTestGreaterEq, OperationType.TestGeq },
                    { Token.TokenType.OpTestLessEq, OperationType.TestLeq },
                    { Token.TokenType.OpIncrement, OperationType.Increment },
                    { Token.TokenType.OpDecrement, OperationType.Decrement },
                    { Token.TokenType.OpNot, OperationType.Not },
                    { Token.TokenType.OpXor, OperationType.Xor },
                    { Token.TokenType.OpShiftLeft, OperationType.ShiftLeft },
                    { Token.TokenType.OpShiftRight, OperationType.ShiftRight },
                    { Token.TokenType.OpDereference, OperationType.Dereference },
                    { Token.TokenType.OpAddressOf, OperationType.AddressOf }
                };

                return lookup[opToken.type];
            }

            public static bool IsTwoParam(OperationType opType) {
                return new List<OperationType>() {
                    OperationType.Addition, OperationType.Subtraction, OperationType.TestEq,
                    OperationType.TestGt, OperationType.TestLt, OperationType.TestGeq,
                    OperationType.TestLeq, OperationType.Xor, OperationType.ShiftLeft,
                    OperationType.ShiftRight
                }.Contains(opType);
            }
        }

            class SingleParamOperation : Operation {
                public Expression operand;

                public override string ToString() {
                    return "[Operation " + this.operationType.ToString() + " " + this.operand.ToString() + "]";
                }
            }

            class TwoParamOperation : Operation {
                public List<Expression> operands;

                public override string ToString() {
                    return "[Operation " + this.operationType.ToString() + " "
                        + this.operands[0].ToString() + " " + this.operands[1].ToString() + "]";
                }
            }

        class LiteralConstant : Expression {
            public int value;

            public override string ToString() {
                return "[Literal " + this.value + "]";
            }
        }

        class Variable : Expression {
            public string name;
            public VarType type;

            public enum VarType {
                Int, IntPtr, Void
            }

            public override bool Equals(object obj) {
                if (!(obj is Variable)) {
                    return false;
                } else {
                    return (obj as Variable).name == this.name;
                }
            }

            public override int GetHashCode() {
                return this.name.GetHashCode();
            }

            public override string ToString() {
                return "[Variable " + this.type.ToString() + " " + this.name + "]";
            }
        }

            class GlobalVariable : Variable {
                public int initValue;

                public override string ToString() {
                    return "[GlobalVariable " + this.type.ToString() + " " + this.name + "]";
                }
            }
    
    interface Action { }

        interface Instruction : Action { }

            class FunctionCall : Instruction, Expression {
                public Function function;
                public List<Expression> arguments;

                public override string ToString() {
                    string result = "[FunctionCall " + function.ToString();

                    if (arguments.Count > 0) {
                        for (int i = 0; i < arguments.Count; i++) {
                            result += " " + arguments[i].ToString();
                            if (i < arguments.Count) {
                                result += ",";
                            }
                        }
                    }

                    return result;
                }
            }

            class InlineAssembly : Instruction {
                public List<string> code;

                public override string ToString() {
                    return "[InlineAssembly]";
                }
            }

            class ReturnInstruction : Instruction {
                public Expression returnValue;

                public override string ToString() {
                    return "[Return " + (returnValue?.ToString() ?? "") + "]";
                }
            }

            class LocalVarDeclaration : Instruction {
                public Variable variable;
                public Expression initValue;

                public override string ToString() {
                    return "[LocalVarDeclaration " + this.variable.ToString() + " " + this.initValue.ToString() + "]";
                }
            }

            class VarAssignment : Instruction {
                public Variable variable;
                public Expression newValue;

                public override string ToString() {
                    return "[VarAssignment " + this.variable.ToString() + this.newValue.ToString() + "]";
                }
            }

                class GlobalVarAssignment : VarAssignment { 
                    new public GlobalVariable variable;
                }

        interface ControlFlowUnit : Action { }

            class If : ControlFlowUnit {
                public Expression condition;
                public List<Action> ifActions;

                public override string ToString() {
                    return "[If " + condition.ToString() + "]";
                }
            }

                class IfElse : If {
                    public List<Action> elseActions;

                    public override string ToString() {
                        return "[IfEsle " + condition.ToString() + "]";
                    }
                }

            class While : ControlFlowUnit {
                public Expression condition;
                public List<Action> actions;

                public override string ToString() {
                    return "[While " + condition.ToString() + "]";
                }
            }

            class For : ControlFlowUnit {
                public Action forSetupAction;
                public Expression forCondition;
                public Action forIncrementAction;

                public override string ToString() {
                    return "[For " + forSetupAction.ToString() 
                        + " " + forCondition.ToString() 
                        + " " + forIncrementAction + "]";
                }
            }

    class Function {
        public string name;
        public Variable.VarType returnsValue;
        public List<Variable> arguments;
        public List<Action> actions;
        public List<Variable> localVars;

        public override string ToString() {
            return "[Function " + name + " --> " + returnsValue.ToString() + "]";
        }
    }

    class AbstractProgram {
        public List<Function> functions;
        public List<GlobalVariable> globalVars;

        public AbstractProgram() {
            this.functions = new List<Function>();
            this.globalVars = new List<GlobalVariable>();
        }
    }
}