using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace AbpHelper.Steps.CSharp
{
    public class ModificationCreatorStep : Step
    {
        public string File { get; set; } = string.Empty;
        public IList<ModificationBuilder> ModificationBuilders { get; set; } = new List<ModificationBuilder>();

        protected override async Task RunStep()
        {
            var sourceFile = File.IsNullOrEmpty() ? GetParameter<string>("FilePathName") : File;
            LogInput(() => sourceFile);

            var sourceText = await System.IO.File.ReadAllTextAsync(sourceFile);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetCompilationUnitRoot();

            var modifications = ModificationBuilders
                .Where(builder => builder.ModifyCondition(root))
                .Select(builder => builder.Build(root))
                .ToList();
            SetParameter("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");
        }
    }
}