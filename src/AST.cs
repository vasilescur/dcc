///<summary>An Abstract Syntax Tree is an abstract representation of a program using
///a tree, with nodes being semantic units of evaluation. This class contains a rich
///object-oriented schema for defining Abstract Syntax Trees.
///</summary>

using System;
using System.Collections;
using System.Collections.Generic;

namespace DCC {
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
            }

            class TwoParamOperation : Operation {
                public List<Expression> operands;
            }

        class LiteralConstant : Expression {
            public int value;
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
                return "[" + this.type.ToString() + " " + this.name + "]";
            }
        }

            class GlobalVariable : Variable {
                public int initValue;
            }
    
    interface Action { }

        interface Instruction : Action { }

            class FunctionCall : Instruction, Expression {
                public Function function;
                public List<Expression> arguments;
            }

            class InlineAssembly : Instruction {
                public List<string> code;
            }

            class ReturnInstruction : Instruction {
                public Expression returnValue;
            }

            class LocalVarDeclaration : Instruction {
                public Variable variable;
                public int initValue;
            }

            class VarAssignment : Instruction {
                public Variable variable;
                public Expression newValue;
            }

                class GlobalVarAssignment : VarAssignment { 
                    new public GlobalVariable variable;
                }

        interface ControlFlowUnit : Action { }

            class If : ControlFlowUnit {
                public Expression condition;
                public List<Action> ifActions;
            }

                class IfElse : If {
                    public List<Action> elseActions;
                }

            class While : ControlFlowUnit {
                public Expression condition;
                public List<Action> actions;
            }

            class For : ControlFlowUnit {
                public Action forSetupAction;
                public Expression forCondition;
                public Action forIncrementAction;
            }

    class Function {
        public string name;
        public Variable.VarType returnsValue;
        public List<Variable> arguments;
        public List<Action> actions;
        public List<Variable> localVars;
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