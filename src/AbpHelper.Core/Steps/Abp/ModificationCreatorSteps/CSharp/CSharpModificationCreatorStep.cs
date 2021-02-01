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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public abstract class CSharpModificationCreatorStep : Step
    {
        protected TextGenerator TextGenerator;

        protected CSharpModificationCreatorStep(TextGenerator textGenerator)
        {
            TextGenerator = textGenerator;
        }

        public WorkflowExpression<string> SourceFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var file = await context.EvaluateAsync(SourceFile, cancellationToken);
            LogInput(() => file);

            var sourceText = await File.ReadAllTextAsync(file, cancellationToken);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetCompilationUnitRoot();

            var builders = CreateModifications(context, root);
            var modifications = builders
                    .Where(builder => builder.ModifyCondition(root))
                    .Select(builder => builder.Build(root))
                    .ToList()
                ;

            context.SetLastResult(modifications);
            context.SetVariable("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Done();
        }

        protected abstract IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit);
    }
}