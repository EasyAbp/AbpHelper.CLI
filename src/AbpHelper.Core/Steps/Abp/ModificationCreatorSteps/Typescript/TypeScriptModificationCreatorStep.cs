using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.Typescript
{
    public abstract class TypeScriptModificationCreatorStep : Step
    {
        protected TextGenerator TextGenerator;

        protected TypeScriptModificationCreatorStep(TextGenerator textGenerator)
        {
            TextGenerator = textGenerator;
        }

        public WorkflowExpression<string> SourceFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context,
            CancellationToken cancellationToken)
        {
            var file = await context.EvaluateAsync(SourceFile, cancellationToken);
            LogInput(() => file);

            var lines = (await File.ReadAllLinesAsync(file, cancellationToken))
                    .Select((l, s) => new LineNode(l, s + 1))
                ;

            var builders = CreateModifications(context);

            var modifications = builders
                    .Where(builder => builder.ModifyCondition(lines))
                    .Select(builder => builder.Build(lines))
                    .ToList()
                ;

            context.SetLastResult(modifications);
            context.SetVariable("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Done();
        }

        protected abstract IList<ModificationBuilder<IEnumerable<LineNode>>> CreateModifications(WorkflowExecutionContext context);
    }
}