using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Generator;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Steps.Common
{
    public class TextGenerationStep : Step
    {
        public const string DefaultGeneratedTextParameterName = "GeneratedText";

        public string TemplateName
        {
            get => GetState<string>();
            set => SetState(value);
        }

        public WorkflowExpression<object> Model
        {
            get => GetState<WorkflowExpression<object>>(() => new JavaScriptExpression<object>("Model"));
            set => SetState(value);
        }

        public WorkflowExpression<string> GeneratedTextKey
        {
            get => GetState(() => new LiteralExpression(DefaultGeneratedTextParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            LogInput(() => TemplateName);
            var model = await context.EvaluateAsync(Model, cancellationToken);
            LogInput(() => model);
            var generatedTextKey = await context.EvaluateAsync(GeneratedTextKey, cancellationToken);
            LogInput(() => GeneratedTextKey);

            var text = TextGenerator.GenerateByTemplateName(TemplateName, model);

            context.SetLastResult(text);
            context.SetVariable(generatedTextKey, text);
            LogOutput(() => text, $"Length: {text.Length}");

            return Done();
        }
    }
}