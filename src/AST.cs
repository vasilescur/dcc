using System;
using System.Collections;
using System.Collections.Generic;

namespace DCC {
    abstract class Expression { }

        abstract class Operation : Expression {
            public OperationType operationType;

            public enum OperationType {
                Addition, Subtraction,
                TestEq, TestGt, TestLt, TestGeq, TestLeq,
                Increment, Decrement,
                Not, Xor, ShiftLeft, ShiftRight,
                Dereference, AddressOf
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
        }

            class GlobalVariable : Variable {
                public int initValue;
            }
    
    abstract class Action { }

        class Instruction : Action {
            public Instruction.Type instructionType;
            public List<Expression> parameters;

            public enum Type {
                Assignment, VarDeclare, FuncCall, Return, Assembly
            }
        }

        abstract class ControlFlowUnit : Action { }

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
        public Type returnsValue;
        public List<Variable> arguments;
        public List<Action> actions;
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