using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Workflow;
using Microsoft.CodeAnalysis.CSharp;

namespace AbpHelper.Steps.CSharp
{
    public class ModificationCreatorStep : Step
    {
        public ModificationCreatorStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public IList<ModificationBuilder> ModificationBuilders { get; set; } = new List<ModificationBuilder>();
        public string SourceFile { get; set; } = string.Empty;

        protected override Task RunStep()
        {
            LogInput(() => SourceFile);
            var sourceText = File.ReadAllText(SourceFile);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetCompilationUnitRoot();

            var modifications = ModificationBuilders.Select(builder => builder.Build(root)).ToList();
            SetParameter("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Task.CompletedTask;
        }
    }
}