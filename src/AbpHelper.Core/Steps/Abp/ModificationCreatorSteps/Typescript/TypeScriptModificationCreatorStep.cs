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

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.Typescript
{
    public abstract class TypeScriptModificationCreatorStep : CodeModificationStepBase
    {
        protected TypeScriptModificationCreatorStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            TemplateDirectory ??= context.GetVariable<string>(VariableNames.TemplateDirectory)!;
            SourceFile ??= context.GetVariable<string>(FileFinderStep.DefaultFileParameterName)!;

            LogInput(() => TemplateDirectory);
            LogInput(() => SourceFile);

            var lines = (await File.ReadAllLinesAsync(SourceFile, context.CancellationToken))
                    .Select((l, s) => new LineNode(l, s + 1))
                ;

            var builders = CreateModifications(context);

            var modifications = builders
                    .Where(builder => builder.ModifyCondition(lines))
                    .Select(builder => builder.Build(lines))
                    .ToList()
                ;

            context.Output = modifications;
            context.SetVariable("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Done();
        }

        protected abstract IList<ModificationBuilder<IEnumerable<LineNode>>> CreateModifications(
            ActivityExecutionContext context);
    }
}