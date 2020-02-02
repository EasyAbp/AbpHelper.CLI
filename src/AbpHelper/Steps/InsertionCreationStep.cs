using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Models;
using AbpHelper.Workflow;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AbpHelper.Steps
{
    public class InsertionCreationStep : Step
    {
        private const string Modifications = "Modifications";

        public InsertionCreationStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public string SourceFile { get; set; } = string.Empty;
        public Func<CompilationUnitSyntax, int> StartLineFunc { get; set; } = root => 0;
        public string Content { get; set; } = string.Empty;
        public InsertPosition InsertPosition { get; set; } = InsertPosition.Before;

        protected override Task RunStep()
        {
            LogInput(() => SourceFile);
            LogInput(() => StartLineFunc);
            LogInput(() => InsertPosition);
            LogInput(() => Content, $"Length: {Content.Length}");

            var sourceText = File.ReadAllText(SourceFile);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetCompilationUnitRoot();
            var insertion = new Insertion(StartLineFunc(root), Content, InsertPosition);

            if (!ContainsParameter(Modifications)) SetParameter(Modifications, new List<Modification>());

            var modifications = GetParameter<IList<Modification>>(Modifications);
            modifications.Add(insertion);

            return Task.CompletedTask;
        }
    }
}