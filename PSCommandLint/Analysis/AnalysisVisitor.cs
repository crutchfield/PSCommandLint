using PSCommandLint.Model;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PSCommandLint.Analysis
{
    /// <summary>
    /// Visitor (based on the built-in powershell visitor) that traverses a script's
    /// abstract syntax tree (AST), looking for invocations of commands that don't exist.
    /// </summary>
    internal class AnalysisVisitor : AstVisitor2
    {
        public AnalysisVisitor(string directory,
            IImmutableDictionary<string, FunctionDefinitionAst> localFunctions,
            IImmutableDictionary<string, CommandInfo> moduleFunctions)
        {
            Directory = directory;
            LocalCommands = localFunctions;
            ModuleCommands = moduleFunctions;
        }

        private string Directory { get; }
        public IImmutableDictionary<string, FunctionDefinitionAst> LocalCommands { get; set; }
        public IImmutableDictionary<string, CommandInfo> ModuleCommands { get; set; }
        public List<ParseError> ValidationErrors = new List<ParseError>();
        public List<ParseError> UnsupportedErrors = new List<ParseError>();

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            // Track that we've seen the function, but don't analyze the body.
            // the body will be analyzed when the function is called.
            // If we tried to analyze the body now, we may find invocations of
            // functions that will be defined later.
            if(LocalCommands.ContainsKey(functionDefinitionAst.Name))
            {
                ValidationError("Overwriting existing function", functionDefinitionAst.Extent);
            }
            LocalCommands = LocalCommands.SetItem(functionDefinitionAst.Name, functionDefinitionAst);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            string commandName = commandAst.GetCommandName();
            if (commandName == null)
            {
                UnsupportedError("Could not get command name", commandAst.Extent);
                return base.VisitCommand(commandAst);
            }

            if (commandAst.InvocationOperator == TokenKind.Dot)
            {
                AnalyzeDotSourcedFile(commandName);
            }
            else if (commandName == "Import-Module" || commandName == "ipmo")
            {
                // TODO
            }
            else
            {
                AnalyzeCommandInvocation(commandAst, commandName);
            }

            return base.VisitCommand(commandAst);
        }

        private void AnalyzeCommandInvocation(CommandAst commandAst, string commandName)
        {
            bool isModuleFunction = ModuleCommands.ContainsKey(commandName);
            if (isModuleFunction)
            {
                // nothing to analyze, it's in another module.
                return;
            }

            bool isLocalFunction = LocalCommands.TryGetValue(commandName, out var functionDefinition);
            if (isLocalFunction)
            {
                // visit the invoked function's body
                var result = CommandAnalyzer.AnalyzeAst(this.Directory, functionDefinition.Body, LocalCommands, ModuleCommands);
                MergeResult(result);
                return;
            }

            ValidationError(commandName + " is not defined", commandAst.Extent);
        }

        private void AnalyzeDotSourcedFile(string filename)
        {
            // invoke a new analysis on the file, and merge the results with
            // the current analysis's results.
            string path = Path.GetFullPath(Path.Combine(this.Directory, filename));

            var result = CommandAnalyzer.AnalyzeFile(path, LocalCommands, ModuleCommands);

            MergeResult(result);
        }

        private void MergeResult(AnalysisResult result)
        {
            this.UnsupportedErrors.AddRange(result.UnsupportedErrors);
            this.ValidationErrors.AddRange(result.ValidationErrors);
            this.LocalCommands = result.LocalCommands.ToImmutableDictionary();
            this.ModuleCommands = result.ModuleCommands.ToImmutableDictionary();
        }

        private void UnsupportedError(string error, IScriptExtent extent) =>
            UnsupportedErrors.Add(new ReportableError(extent, error));

        private void ValidationError(string error, IScriptExtent extent) =>
            ValidationErrors.Add(new ReportableError(extent, error));
    }
}
