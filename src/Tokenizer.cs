///<summary>
///The Tokenizer is responsible for reading the raw source code and converting it to
///a series of <c>Token</c> objects, essentially translating syntax into distinct parts
///of code.
///</summary>
///<author>Radu Vasilecu</author>
///<date>2019-03-29</date>

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using static DCC.Token.TokenType;

namespace DCC {
    ///<summary>Represents one semantic unit of the source program.
    ///</summary>
    class Token {
        public string content;
        public TokenType type;

        public Token(TokenType type, string content) {
            this.content = content;
            this.type = type;
        }

        public override string ToString() {
            StringBuilder result = new StringBuilder("");

            result.Append("[");
            result.Append(Enum.GetName(typeof(TokenType), this.type));
            result.Append(" " + this.content + "]");

            return result.ToString();
        }

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
            KwIf, KwElse,
            KwFor, KwWhile,
            KwReturn,

            // Labels/Goto
            LabelDef, LabelName, KwGoto,

            // Inline Assembly
            Assembly,
        }

        ///<summary>Checks whether this <c>Token</c> is one of the listed types of expression
        ///operators.
        ///</summary>
        public static bool IsExpressionOperator(Token token) {
            return new List<TokenType> {
                OpTestEq, OpTestGreater, OpTestLess, OpTestGreaterEq, OpTestLessEq,
                OpPlus, OpMinus, OpNot, OpXor, OpShiftLeft, OpShiftRight
            }.Contains(token.type);
        }
    }

    class Tokenizer {
        string fullSource;

        Token lastToken;

        // Identifiers
        List<string> functions = new List<string>();
        List<string> variables = new List<string>();
        List<string> labels = new List<string>();

        static Regex validIdentifier = new Regex(@"[_a-zA-Z][_a-zA-Z0-9]*");

        public List<Token> Tokenize(List<string> source) {
            // First, we need to get rid of needless whitespace, like line breaks between commands.
            this.fullSource = String.Join("", source);

            List<Token> result = new List<Token>();

            // Then, tokenize!
            StringBuilder restSource = new StringBuilder(fullSource);

            //TODO: Debug only
            int done = 0;

            do {
                // Fast-forward to the beginning of the next token, if needed
                restSource = restSource.TrimStart();

                Token nextToken = consumeNextToken(ref restSource);
                lastToken = nextToken;
                result.Add(nextToken);
                done++;
            } while (result.Last().type != EOF && done < 100);

            return result;
        }

        private Token consumeNextToken(ref StringBuilder restSource) {
            if (restSource.Length == 0) {
                return new Token(EOF, "");
            }

            //* Punctuation

            else if (restSource.TrimStart().StartsWith("{")) {
                return new Token(BraceOpen, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("}")) {
                return new Token(BraceClose, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("(")) {
                return new Token(ParenOpen, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith(")")) {
                return new Token(ParenClose, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("[")) {
                return new Token(BracketOpen, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("]")) {
                return new Token(BracketClose, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith(";")) {
                return new Token(Semicolon, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith(",")) {
                return new Token(Comma, restSource.Consume(1));
            }

            //* Operators

            else if (restSource.TrimStart().StartsWith("=") && !restSource.TrimStart().StartsWith("==")) {
                return new Token(OpAssign, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("==")) {
                return new Token(OpTestEq, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith(">") && !restSource.TrimStart().StartsWith(">=")) {
                return new Token(OpTestGreater, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("<") && !restSource.TrimStart().StartsWith("<=")) {
                return new Token(OpTestLess, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith(">=")) {
                return new Token(OpTestGreaterEq, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith("<=")) {
                return new Token(OpTestLessEq, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith("+")
                        && !restSource.TrimStart().StartsWith("+=")
                        && !restSource.TrimStart().StartsWith("++")) {
                return new Token(OpPlus, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("-")
                        && !restSource.TrimStart().StartsWith("-=")
                        && !restSource.TrimStart().StartsWith("--")) {
                // Is it a negative literal int?
                if (restSource.Substring(1, 1).ToString().All(char.IsDigit)) {
                    string literal = restSource.Consume(1) + restSource.ConsumeWhile(c => char.IsDigit(c));
                    return new Token(LiteralInt, literal);
                } else {
                    // It's a minus operator
                    return new Token(OpMinus, restSource.Consume(1));
                }
            } else if (restSource.TrimStart().StartsWith("+=")) {
                return new Token(OpPlusEquals, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith("-=")) {
                return new Token(OpMinusEquals, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith("++")) {
                return new Token(OpIncrement, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith("--")) {
                return new Token(OpDecrement, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith("~")) {
                return new Token(OpNot, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("^")) {
                return new Token(OpXor, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("<<")) {
                return new Token(OpShiftLeft, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith(">>")) {
                return new Token(OpShiftRight, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith("*")) {
                return new Token(OpDereference, restSource.Consume(1));
            } else if (restSource.TrimStart().StartsWith("&")) {
                return new Token(OpAddressOf, restSource.Consume(1));
            }

            //* Literals

            else if (char.IsDigit(restSource.TrimStart().ToString()[0])) {
                return new Token(LiteralInt, restSource.ConsumeWhile(c => char.IsDigit(c)));
            } else if (restSource.TrimStart().StartsWith("'")) {
                Token result = new Token(LiteralChar, restSource.ConsumeUntil("'"));
                restSource.Consume(1);     // Get rid of traling '
                return result;
            }

            //* Type names

            else if (restSource.TrimStart().StartsWith("int*")
                        || restSource.TrimStart().StartsWith("int")
                        || restSource.TrimStart().StartsWith("void")) {
                return new Token(TypeName, restSource.ConsumeUntil(" "));
            }

            //* Keywords

            //TODO: Bug if naming a variable something like `returnValue`. StartsWith("xxx ") ?
            else if (restSource.TrimStart().StartsWith("if")) {
                return new Token(KwIf, restSource.Consume(2));
            } else if (restSource.TrimStart().StartsWith("else")) {
                return new Token(KwElse, restSource.Consume(4));
            } else if (restSource.TrimStart().StartsWith("for")) {
                return new Token(KwFor, restSource.Consume(3));
            } else if (restSource.TrimStart().StartsWith("return")) {
                return new Token(KwReturn, restSource.Consume("return".Length));
            } else if (restSource.TrimStart().StartsWith("asm")) {
                // Manually tokenize the entire inline assembly.
                restSource.ConsumeUntil("\"");
                restSource.Consume(1);

                restSource = restSource.TrimStart();

                string inlineAsm = restSource.ConsumeUntil("\"");
                inlineAsm = inlineAsm.Replace(';', '\n');

                restSource.ConsumeUntil(";");
                restSource.Consume(1);

                return new Token(Assembly, inlineAsm);
            }

            //* Labels and Goto

            else if (restSource.TrimStart().StartsWith("goto")) {
                return new Token(KwGoto, restSource.Consume("goto".Length));
            } 
            // Labels are like: labelname: code. Therefore, since label names can't have spaces, we take a 
            // shortcut-- if the first ":" occurs before the first " ", it's a label!
            else if (restSource.ToString().Contains(":") && 
                        restSource.ToString().IndexOf(":") < restSource.ToString().IndexOf(" ")) {
                string newLabel = restSource.ConsumeUntil(":");
                labels.Add(newLabel);

                restSource.Consume(1);  // Get rid of the ":"
                
                return new Token(LabelDef, newLabel);
            }

            //* Variables and Functions and Label Names
            else {
                restSource = restSource.TrimStart();

                // Variable names and function names must either come after a type name or already be declared.
                if (lastToken.type == TypeName) {
                    string newIdent = restSource.ConsumeWhile(c => validIdentifier.IsMatch(c.ToString()));

                    // If the next relevant char is an open paren, it's a function. Otherwise, it's a var.
                    if (restSource.TrimStart().StartsWith("(")) {
                        // Has to be a function def, since it follows a type name. Therefore, it can't already 
                        // have been seen.
                        functions.Add(newIdent);
                        return new Token(FuncName, newIdent);
                    } else {
                        // It's a variable. Since it follows a type name, it's gotta be new
                        variables.Add(newIdent);
                        return new Token(VarName, newIdent);
                    }                    
                }

                // Label names must come after "goto ". If prev == Goto, --> this is a label name
                if (lastToken.type == KwGoto) {
                    return new Token(LabelName, restSource.ConsumeUntil(";"));
                }

                // Otherwise, it's gotta be either a function name or a var name
                string identifier = restSource.ConsumeWhile(c => validIdentifier.IsMatch(c.ToString()));

                if (functions.Contains(identifier)) {
                    return new Token(FuncName, identifier);
                } else if (variables.Contains(identifier)) {
                    return new Token(VarName, identifier);
                }

                // If we've gotten here, uh oh.
                throw new TokenException("Error: Unknown identifier \"" + identifier + "\"");
            }
        }

        [System.Serializable]
        public class TokenException : System.Exception {
            public TokenException() { }
            public TokenException(string message) : base(message) { }
            public TokenException(string message, System.Exception inner) : base(message, inner) { }
            protected TokenException(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}