using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.ActivityResults;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public abstract class CSharpModificationCreatorStep : CodeModificationStepBase
    {
        protected CSharpModificationCreatorStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            TemplateDirectory ??= context.GetVariable<string>(VariableNames.TemplateDirectory)!;
            SourceFile ??= context.GetVariable<string>(FileFinderStep.DefaultFileParameterName)!;

            LogInput(() => TemplateDirectory);
            LogInput(() => SourceFile);

            var sourceText = await File.ReadAllTextAsync(SourceFile!, context.CancellationToken);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetCompilationUnitRoot();

            var builders = CreateModifications(context, root);
            var modifications = builders
                    .Where(builder => builder.ModifyCondition(root))
                    .Select(builder => builder.Build(root))
                    .ToList()
                ;

            context.Output = modifications;
            context.SetVariable("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Done();
        }

        protected abstract IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit);
    }
}