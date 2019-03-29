using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCC {
    class Token {
        public string content;

        public enum TokenType {
            EOF,

            // Braces, parentheses, etc
            BraceOpen, BraceClose,
            ParenOpen, ParenClose,
            BracketOpen, BracketClose,
            Semicolon, Comma,

            // Operators
            OpAssign,
            OpTestEq, OpTestGreater, OpTestLess, OpTestGreaterEq, OpTestLessEq,
            OpPlus, OpMinus,
            OpPlusEquals, OpMinusEquals,
            OpIncrement, OpDecrement,
            OpNot, OpXor,
            OpShiftLeft, OpShiftRight,
            OpDereference, OpAddressOf,

            // Literals
            LiteralInt,
            LiteralChar,
            
            // Type names
            TypeName,

            // Variables and Functions
            VarName,
            FuncName,

            // Keywords
            KwIf, KyElse,
            KwFor, KwWhile,
            KwReturn,
            KwAsm,

            // Labels/Goto
            Label, KwGoto
        }

        public Token(string content) {
            this.content = content;
        }
    }

    static class Tokenizer {
        public static List<Token> Tokenize(List<string> source) {
            throw new System.NotImplementedException("foo");
        }
    }
}