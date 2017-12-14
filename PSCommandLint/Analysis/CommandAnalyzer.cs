using PSCommandLint.Model;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace PSCommandLint.Analysis
{
    public class CommandAnalyzer 
    {
        /// <summary>
        /// Analyze a powershell file, and report attempted invocations of functions that don't exist.
        /// </summary>
        /// <param name="filename">the powershell file to analyze</param>
        /// <returns>A report of attempted invocations of missing functions</returns>
        public static AnalysisResult AnalyzeFile(string filename)
        {
            var inScopeFunctions = GetInScopeFunctions();

            return AnalyzeFile(filename,
                ImmutableDictionary<string, FunctionDefinitionAst>.Empty,
                inScopeFunctions);
        }

        /// <summary>
        /// Analyze a powershell file, and report attempted invocations of functions that don't exist.
        /// </summary>
        /// <param name="filename">the powershell file to analyze</param>
        /// <param name="localFunctions">An initial set of in-scope functions that are defined by the script</param>
        /// <param name="moduleFunctions">An initial set of in-scope functions that are globally defined</param>
        /// <returns>A report of attempted invocations of missing functions</returns>
        internal static AnalysisResult AnalyzeFile(string filename,
            IImmutableDictionary<string, FunctionDefinitionAst> localFunctions,
            IImmutableDictionary<string, CommandInfo> moduleFunctions)
        {
            string directory = Path.GetDirectoryName(filename);
            var ast = Parser.ParseFile(
                filename,
                out Token[] tokens,
                out ParseError[] errors
            );

            return AnalyzeAst(directory, ast, localFunctions, moduleFunctions, errors);
        }

        /// <summary>
        /// Analyze a powershell abstract syntax tree (AST), and report attempted invocations of functions that don't exist.
        /// </summary>
        /// <param name="directory">The directory that contains the original script. Used for resolving dot-sourced files.</param>
        /// <param name="ast">the powershell AST, obtained from the standard powershell <see cref="Parser"/>.</param>
        /// <param name="localFunctions">An initial set of in-scope functions that are defined by the script</param>
        /// <param name="moduleFunctions">An initial set of in-scope functions that are globally defined</param>
        /// <param name="errors">Any errors that were present in the parsing that produced the AST, to be included in the analysis result.</param>
        /// <returns>A report of attempted invocations of missing functions</returns>
        internal static AnalysisResult AnalyzeAst(string directory, ScriptBlockAst ast,
            IImmutableDictionary<string, FunctionDefinitionAst> localFunctions,
            IImmutableDictionary<string, CommandInfo> moduleFunctions,
            ParseError[] errors = null)
        {
            var visitor = new AnalysisVisitor(directory, localFunctions, moduleFunctions);
            ast.Visit(visitor);

            return new AnalysisResult(errors ?? new ParseError[0],
                visitor.ValidationErrors, visitor.UnsupportedErrors,
                visitor.LocalCommands, visitor.ModuleCommands);
        }

        /// <summary>
        /// Get all the functions available globally in powershell, plus in the provided modules.
        /// </summary>
        private static IImmutableDictionary<string, CommandInfo> GetInScopeFunctions(params string[] modules)
        {
            var ps = PowerShell.Create();
            foreach (var module in modules)
            {
                ps.Commands.AddCommand("Import-Module").AddArgument(module);
            }
            if(ps.Commands.Commands.Any())
            {
                ps.Invoke();
            }
            var commands = ps.Runspace.SessionStateProxy.InvokeCommand.GetCommands("*", CommandTypes.All, true);
            return commands
                // some entries, like notepad.exe appear twice in our commands, so we take the first of each group
                .ToLookup(command => command.Name)
                .ToDictionary(group => group.Key, group => group.First())
                .ToImmutableDictionary();
        }
    }
}
