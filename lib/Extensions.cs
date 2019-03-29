// Custom extensions to library classes for convenience.

using System;
using System.Text;

namespace DCC {
    public static class StringBuilderExtension {
        public static string Substring(this StringBuilder value, int startPos, int length) {
            return value.ToString().Substring(startPos, length);
        }

        public static bool StartsWith(this StringBuilder sb, string value) {
            return sb.ToString().StartsWith(value);
        }

        public static string Consume(this StringBuilder sb, int length) {
            string result = sb.Substring(0, length);
            sb.Remove(0, length);
            return result;
        }

        public static string ConsumeUntil(this StringBuilder sb, string sentinel) {
            StringBuilder result = new StringBuilder("");
            int length = sb.ToString().IndexOf(sentinel);
            return sb.Consume(length);
        }

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

        public static StringBuilder TrimStart(this StringBuilder sb) {
            return new StringBuilder(sb.ToString().TrimStart());
        }
    }
}