using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    public class TextGenerationStep : Step
    {
        private readonly TextGenerator _textGenerator;
        public const string DefaultGeneratedTextParameterName = "GeneratedText";

        public WorkflowExpression<string> TemplateDirectory
        {
            get => GetState<WorkflowExpression<string>>(() => new JavaScriptExpression<string>(VariableNames.TemplateDirectory));
            set => SetState(value);
        }
        
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

        public TextGenerationStep(TextGenerator textGenerator)
        {
            _textGenerator = textGenerator;
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            string templateDir = await context.EvaluateAsync(TemplateDirectory, cancellationToken);
            LogInput(() => templateDir);
            LogInput(() => TemplateName);
            var model = await context.EvaluateAsync(Model, cancellationToken);
            LogInput(() => model);
            var generatedTextKey = await context.EvaluateAsync(GeneratedTextKey, cancellationToken);
            LogInput(() => GeneratedTextKey);

            var text = _textGenerator.GenerateByTemplateName(templateDir, TemplateName, model);

            context.SetLastResult(text);
            context.SetVariable(generatedTextKey, text);
            LogOutput(() => text, $"Length: {text.Length}");

            return Done();
        }
    }
}