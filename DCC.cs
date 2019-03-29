using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCC {
    class DCC {
        static Program program;

        static void Main(string[] args) {
            System.Console.WriteLine("\nDuke 250/16 C Compiler version 1.0 - Radu Vasilescu, 2019\n");

            if (args.Length != 1) {
                System.Console.WriteLine();
                ShowHelpText();
                return;
            }

            // Read the source file
            string[] sourceStr;
            List<string> source = null;

            try {
                sourceStr = File.ReadAllLines(args[0]);
                source = sourceStr.OfType<string>().ToList();
            } catch (Exception e) {
                Console.Error.WriteLine("Error: " + e.Message);
                Environment.Exit(1);
            }

            // Run the pre-processor
            source = Preprocessor.ProcessAll(source);

            // Print the source
            PrintSource(source);

            // Tokenize the source
            var tokenized = Tokenizer.Tokenize(source);
        }

        private static void PrintSource(List<string> source) {
            for (int i = 0; i < source.Count; i++) {
                System.Console.WriteLine(source[i]);
            }
        }

        static void ShowHelpText() {
            System.Console.WriteLine("Usage: dcc program.c");
            System.Console.WriteLine();
        }
    }
}
