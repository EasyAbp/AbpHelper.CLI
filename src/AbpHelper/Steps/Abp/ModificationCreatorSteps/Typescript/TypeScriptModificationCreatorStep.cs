using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Generator;
using DosSEdo.AbpHelper.Models;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.Typescript
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
            string file = await context.EvaluateAsync(SourceFile, cancellationToken);
            LogInput(() => file);

            IEnumerable<LineNode> lines = (await File.ReadAllLinesAsync(file, cancellationToken))
                    .Select((l, s) => new LineNode(l, s + 1))
                ;

            IList<ModificationBuilder<IEnumerable<LineNode>>> builders = CreateModifications(context);

            List<Modification> modifications = builders
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