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
using Microsoft.CodeAnalysis.CSharp;

namespace DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
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
            string file = await context.EvaluateAsync(SourceFile, cancellationToken);
            LogInput(() => file);

            string sourceText = await File.ReadAllTextAsync(file, cancellationToken);
            Microsoft.CodeAnalysis.SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
            Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            IList<ModificationBuilder<CSharpSyntaxNode>> builders = CreateModifications(context);
            List<Modification> modifications = builders
                    .Where(builder => builder.ModifyCondition(root))
                    .Select(builder => builder.Build(root))
                    .ToList()
                ;

            context.SetLastResult(modifications);
            context.SetVariable("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Done();
        }

        protected abstract IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context);
    }
}