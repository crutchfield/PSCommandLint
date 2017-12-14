using System.Collections.Generic;
using System.Collections.Immutable;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PSCommandLint.Model
{
    public class AnalysisResult
    {
        public AnalysisResult(
            IList<ParseError> parseErrors,
            IList<ParseError> validationErrors,
            IList<ParseError> unsupportedErrors,
            IReadOnlyDictionary<string, FunctionDefinitionAst> localFunctions,
            IReadOnlyDictionary<string, CommandInfo> moduleFunctions)
        {
            ParseErrors = parseErrors;
            ValidationErrors = validationErrors;
            UnsupportedErrors = unsupportedErrors;
            LocalCommands = localFunctions;
            ModuleCommands = moduleFunctions;
        }

        /// <summary>
        /// An error in the powershell script, e.g. an attempted invocation of a function that does not exist.
        /// </summary>
        public IList<ParseError> ValidationErrors { get; }

        /// <summary>
        /// Report a short-coming of this tool, e.g. some powershell syntax we don't or can't support.
        /// </summary>
        public IList<ParseError> UnsupportedErrors { get; }

        /// <summary>
        /// Syntax errors encountered while parsing the file
        /// </summary>
        public IList<ParseError> ParseErrors { get; }

        /// <summary>
        /// A dictionary of all user-defined functions in the script.
        /// the function's name -> the function's AST
        /// </summary>
        public IReadOnlyDictionary<string, FunctionDefinitionAst> LocalCommands { get; set; }

        /// <summary>
        /// A dictionary of all module-defined functions in the script.
        /// the function's name -> the function's CommandInfo
        /// </summary>
        public IReadOnlyDictionary<string, CommandInfo> ModuleCommands { get; set; }
    }
}
