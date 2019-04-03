///<summary>
///The Preprocessor manipulates the raw source code before it is tokenized, performing
///operations like removing comments and empty lines and resolving <c>#include</c> statements.
///</summary>
///<author>Radu Vasilecu</author>
///<date>2019-03-29</date>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

namespace dcc {
    static class Preprocessor {
        public static List<string> ProcessAll(List<string> source, bool enableStrout) {
            List<string> result = new List<string>();
            foreach (string line in source) {
                result.Add(line);
            }

            // This order is important.
            ProcessIncludes(result);
            ProcessOutStringMacros(result, enableStrout);
            //TODO: #define statements!
            result = StripComments(result);
            StripWhiteSpace(result);
            StripEmptyLines(result);

            return result;
        }

        private static void StripWhiteSpace(List<string> source) {
            for (int i = 0; i < source.Count; i++) {
                source[i] = source[i].Trim();
            }
        }

        private static void ProcessOutStringMacros(List<string> source, bool enableStrout) {
            for (int i = 0; i < source.Count; i++) {
                if (source[i].Trim().StartsWith("#pragma OutString")) {
                    if (!enableStrout) {
                        source[i] = "";
                        continue;
                    }

                    string command = "";
                    string theString = source[i].Trim().Split("OutString")[1].TrimStart();

                    foreach (char c in theString.ToCharArray()) {
                        command += "putc(" + ((int) c) + "); ";
                    }

                    command += "putc(10);";

                    source[i] = command;
                }
            }
        }

        private static List<string> StripComments(List<string> source) {
            string fullSource = "";

            foreach (string line in source) {
                fullSource += line;
                fullSource += "\n";
            }

            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            string cleaned = Regex.Replace(fullSource,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me => {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);

            return cleaned.Split("\n").OfType<string>().ToList();
        }

        private static void StripEmptyLines(List<string> source) {
            source.RemoveAll(line => line.Length == 0 || line == "\n");
        }

        ///<summary>Appends the content of each include file to the place in the source code where
        ///the <c>#include</c> directive appeared, and removes the directive.
        ///</summary>
        private static void ProcessIncludes(List<string> source) {
            Dictionary<int, string> requestedIncludes = new Dictionary<int, string>();

            for (int i = 0; i < source.Count; i++) {
                if (source[i].StartsWith("#include")) {
                    requestedIncludes.Add(i, source[i].Split(" ")[1].Trim('"'));
                }
            }

            // If no includes --> done
            if (requestedIncludes.Keys.Count == 0) {
                return;
            }

            // Replace the #include lines with the contents
            foreach (var include in requestedIncludes) {
                source.RemoveAt(include.Key);

                List<string> includedContent = null;

                try {
                    includedContent = File.ReadAllLines(include.Value).OfType<string>().ToList();
                } catch (Exception e) {
                    Console.Error.WriteLine("Error including \"" + include.Value + "\": " + e.Message);
                    Environment.Exit(1);
                }

                source.InsertRange(include.Key, includedContent);
            }
        }
    }
}