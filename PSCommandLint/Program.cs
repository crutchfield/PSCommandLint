using PSCommandLint.Analysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;
using Debugger = System.Diagnostics.Debugger;

namespace PSCommandLint
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = CommandAnalyzer.AnalyzeFile("./Input/Input.ps1");

            PrintErrors("Parse error found:", result.ParseErrors);
            PrintErrors("Issues Found:", result.ValidationErrors);
            PrintErrors("This tool does not support 100% of powershell syntax. Please check the following areas manually:", result.UnsupportedErrors);

            Console.WriteLine("Complete");
            // prevent console window close when running from an IDE
            if(Debugger.IsAttached) Console.ReadKey();
        }

        private static void PrintErrors(string header, IEnumerable<ParseError> errors)
        {
            if (errors.Any())
            {
                Console.WriteLine(header);
                Console.WriteLine();
                string currentDirectory = Directory.GetCurrentDirectory();
                foreach (var error in errors.Distinct())
                {
                    string relativeFile = error.Extent.File.Replace(currentDirectory, ".");
                    Console.WriteLine(error.Message + " in file " + relativeFile);
                    Console.WriteLine();
                    Console.WriteLine(error.Extent.StartLineNumber + ":    " + error.Extent.Text);
                    Console.WriteLine();
                }
            }
        }

    }
}
