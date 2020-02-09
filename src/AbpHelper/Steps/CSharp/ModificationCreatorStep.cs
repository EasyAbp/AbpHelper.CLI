using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace AbpHelper.Steps.CSharp
{
    public class ModificationCreatorStep : Step
    {
        public WorkflowExpression<string> SourceFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        public IList<ModificationBuilder> ModificationBuilders
        {
            get => GetState<IList<ModificationBuilder>>();
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var file = await context.EvaluateAsync(SourceFile, cancellationToken);
            LogInput(() => file);

            var sourceText = await File.ReadAllTextAsync(file, cancellationToken);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetCompilationUnitRoot();

            var modifications = ModificationBuilders
                .Where(builder => builder.ModifyCondition(root))
                .Select(builder => builder.Build(root, context))
                .ToList();
            context.SetLastResult(modifications);
            context.SetVariable("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Done();
        }
    }
}