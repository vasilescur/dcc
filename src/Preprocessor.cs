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
            ProcessDefines(result);
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

        /// <summary>Parses through the contents of the file to replace all uses of a #define-d keyword
        /// with its value.
        ///</summary>
        ///<author> Jake Derry </author>
        private static void ProcessDefines(List<string> source) {
            Dictionary <string, string> requestedDefines = new Dictionary<string, string>();

            for (int i = 0; i < source.Count; i++) {
                if (source[i].StartsWith("#define")) {
                    // add the define to the requestedDefines
                    string[] splitSourceLine = source[i].Split(" ");

                    string[] defValueSplit = new string[splitSourceLine.Length - 2];
                    Array.Copy(splitSourceLine, 2, defValueSplit, 0, defValueSplit.Length);

                    string defWord = splitSourceLine[1];
                    requestedDefines.Add(defWord, String.Join(" ", defValueSplit));

                    // comment out the line
                    source[i] = "//" + source[i];
                } // if
                else {

                    // skip commented lines
                    if (source[i].StartsWith("//")) continue;

                    List<String> definesSorted = requestedDefines.Keys.ToList();
                    definesSorted.Sort();
                    definesSorted.Reverse();
                    
                    string[] splitSourceLine = source[i].Split(" ");
                    foreach(String defKey in definesSorted) {
                        for (int j = 0; j < splitSourceLine.Length; j++) {
                            if (splitSourceLine[j].StartsWith('"') || 
                            splitSourceLine[j].StartsWith("'")) 
                                continue;

                            if (splitSourceLine[j].Equals(defKey)) 
                                splitSourceLine[j] = requestedDefines[defKey];
                            else {
                                int keyDex = splitSourceLine[j].IndexOf(defKey);
                                int keyDexEnd = keyDex + defKey.Length;
                                if (keyDex != -1) {
                                    splitSourceLine[j] = splitSourceLine[j].Substring(0, keyDex) + 
                                                        requestedDefines[defKey] + 
                                                        splitSourceLine[j].Substring(keyDexEnd, splitSourceLine[j].Length - keyDexEnd);
                                } // if
                            } // else
                        } // for
                    } // foreach
                    source[i] = String.Join(" ", splitSourceLine);
                } // else
                
            } // for
        }
    }
}