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

        protected override Task RunStep()
        {
            var sourceFile = GetParameter<string>("FilePathName");
            LogInput(() => sourceFile);
            var sourceText = File.ReadAllText(sourceFile);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetCompilationUnitRoot();

            var modifications = ModificationBuilders.Select(builder => builder.Build(root)).ToList();
            SetParameter("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Task.CompletedTask;
        }
    }
}