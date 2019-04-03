///<summary>
///Main class and entry point for the C compiler.
///</summary>
///<author>Radu Vasilecu</author>
///<date>2019-03-29</date>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace dcc {
    class dcc {
        static void Main(string[] args) {
            System.Console.WriteLine("\nDuke 250/16 C Compiler version 1.0 - Radu Vasilescu, 2019\n");

            if (args.Length == 0) {
                System.Console.WriteLine();
                ShowHelpText();
                return;
            }

            bool verbose = false;
            bool printPreproc = false;
            bool enableStrout = true;
            string sourceFile = "";

            foreach (string arg in args) {
                if (arg == "-v" || arg == "--verbose") {
                    verbose = true;
                } else if (arg == "-h" || arg == "--help") {
                    ShowHelpText();
                    Environment.Exit(0);
                } else if (arg == "-p" || arg == "--pre-processor") {
                    printPreproc = true;
                } else if (arg == "--disable-strout") {
                    enableStrout = false;
                } else {
                    sourceFile = arg;
                }
            }

            // Read the source file
            string[] sourceStr;
            List<string> source = null;

            try {
                sourceStr = File.ReadAllLines(sourceFile);
                source = sourceStr.OfType<string>().ToList();
            } catch (Exception e) {
                Console.Error.WriteLine("Error: " + e.Message);
                Environment.Exit(1);
            }

            // Run the pre-processor
            source = Preprocessor.ProcessAll(source, enableStrout);

            if (printPreproc) PrintSource(source);

            // Tokenize the source
            Tokenizer toker = new Tokenizer();
            List<Token> tokenized = null;

            try {
                tokenized = toker.Tokenize(source);
            } catch (Tokenizer.TokenException e) {
                Console.Error.WriteLine("Error: " + e.Message);
                Environment.Exit(1);
            }

            if (verbose) System.Console.WriteLine("Parsing...");

            // Parse the tokenized program into a collection of Abstract Source Trees
            Parser parser = new Parser(tokenized);
            AbstractProgram parsedProgram = parser.Parse();

            if (verbose) System.Console.WriteLine("Done parsing.");

            // Generate and emit assembly instructions
            Emitter emitter = new Emitter(parsedProgram);
            List<string> outputCode = emitter.EmitAssembly();

            if (verbose) System.Console.WriteLine("Done emitting.");

            if (verbose) {
                System.Console.WriteLine("\nAssembly: \n");

                foreach (string line in outputCode) {
                    System.Console.WriteLine(line);
                }
            }
            
            string outputFile = sourceFile.Split(".")[0] + ".s";
            string outputCodeStr = String.Join("\n", outputCode);

            File.WriteAllText(outputFile, outputCodeStr);

            if (verbose) System.Console.WriteLine("\nOutput stored in: " + outputFile);
        }

        private static void PrintSource(List<string> source) {
            for (int i = 0; i < source.Count; i++) {
                System.Console.WriteLine(source[i]);
            }
        }

        static void ShowHelpText() {
            System.Console.WriteLine("Usage: dcc [-v | --verbose] program.c");
            System.Console.WriteLine();
            System.Console.WriteLine("Options: ");
            System.Console.WriteLine("    -h    --help          Display this help message.");
            System.Console.WriteLine("    -v    --verbose       Print steps and progress, and output the");
            System.Console.WriteLine("                          assembled result to the console.");
            System.Console.WriteLine();
        }
    }
}
