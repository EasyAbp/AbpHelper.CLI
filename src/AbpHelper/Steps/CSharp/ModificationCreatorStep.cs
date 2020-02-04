using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace AbpHelper.Steps.CSharp
{
    public class ModificationCreatorStep : Step
    {
        public IList<ModificationBuilder> ModificationBuilders { get; set; } = new List<ModificationBuilder>();

        protected override Task RunStep()
        {
            var sourceFile = GetParameter<string>("FilePathName");
            LogInput(() => sourceFile);
            var sourceText = File.ReadAllText(sourceFile);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetCompilationUnitRoot();

            var modifications = ModificationBuilders
                .Where(builder => builder.ShouldModifier(root))
                .Select(builder => builder.Build(root))
                .ToList();
            SetParameter("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Task.CompletedTask;
        }
    }
}