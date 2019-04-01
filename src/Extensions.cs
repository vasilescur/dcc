// Custom extensions to library classes for convenience.

using System;
using System.Text;

namespace DCC {
    public static class StringBuilderExtension {

        ///<summary>Implementation of the <c>String</c> class's <c>Substring</c method, but for the
        /// <s>StringBuilder</c>.
        ///</summary>
        public static string Substring(this StringBuilder value, int startPos, int length) {
            return value.ToString().Substring(startPos, length);
        }

        ///<summary>Returns <c>true</c> if the <c>string</c> contained within this object starts
        /// with a specified <c>string</c>
        ///</summary>
        public static bool StartsWith(this StringBuilder sb, string value) {
            return sb.ToString().StartsWith(value);
        }

        ///<summary>Returns the first <c>length</c> characters from the string, and then removes those
        ///characters.
        ///</summary>
        public static string Consume(this StringBuilder sb, int length) {
            string result = sb.Substring(0, length);
            sb.Remove(0, length);
            return result;
        }

        ///<summary>Consumes until it finds a specified sentinel character, then stops. Does not Consume
        /// the sentinel.
        ///</summary>
        public static string ConsumeUntil(this StringBuilder sb, string sentinel) {
            StringBuilder result = new StringBuilder("");
            int length = sb.ToString().IndexOf(sentinel);
            return sb.Consume(length);
        }

        ///<summary>Keeps Consuming characters while a certain <c>predicate</c> is true, then stops. Does
        ///not consume the character that caused the <c>predicate</c> to return <c>false</c>.
        public static string ConsumeWhile(this StringBuilder sb, Func<char, bool> predicate) {
            StringBuilder result = new StringBuilder("");

            while (true) {
                if (sb.ToString().Length == 0) {
                    return "";
                } else if (predicate.Invoke(sb.ToString()[0])) {
                    result.Append(sb.Consume(1));
                } else {
                    return result.ToString();
                }
            }
        }

        ///<summary>Trims the start of the string of any whitespace.
        ///</summary>
        public static StringBuilder TrimStart(this StringBuilder sb) {
            return new StringBuilder(sb.ToString().TrimStart());
        }
    }
}