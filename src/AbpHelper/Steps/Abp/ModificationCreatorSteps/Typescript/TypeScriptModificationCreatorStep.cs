using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.Common;
using EasyParser.Core;
using EasyParser.TypeScriptParser;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.Typescript
{
    public abstract class TypeScriptModificationCreatorStep : Step 
    {
        private readonly TextParser _textParser;

        public TypeScriptModificationCreatorStep(TextParser textParser)
        {
            _textParser = textParser;
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

            var sourceText = await File.ReadAllTextAsync(file, cancellationToken);

            var parser = _textParser.RegisterParser<Parser>()
                    .SetText(sourceText)
                    .ParseTree()
                ;

            var builders = CreateModifications(context);

            var modifications = builders
                    .Where(builder => builder.ModifyCondition(parser))
                    .Select(builder => builder.Build(parser))
                    .ToList()
                ;

            context.SetLastResult(modifications);
            context.SetVariable("Modifications", modifications);
            LogOutput(() => modifications, $"Modifications count: {modifications.Count}");

            return Done();
        }

        protected abstract IList<ModificationBuilder<IParseTree>> CreateModifications(WorkflowExecutionContext context);
    }
}