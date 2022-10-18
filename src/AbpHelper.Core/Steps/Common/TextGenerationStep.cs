using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    [Activity(
        Category = "TextGenerationStep",
        Description = "TextGenerationStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class TextGenerationStep : Step
    {
        private readonly TextGenerator _textGenerator;
        public const string DefaultGeneratedTextParameterName = "GeneratedText";

        [ActivityInput(
            Hint = "TemplateDirectory",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? TemplateDirectory
        {
            get => GetState<string?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "TemplateName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string TemplateName
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "GeneratedTextKey",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string GeneratedTextKey
        {
            get => GetState(() => DefaultGeneratedTextParameterName);
            set => SetState(value);
        }

        public TextGenerationStep(TextGenerator textGenerator)
        {
            _textGenerator = textGenerator;
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            TemplateDirectory ??= context.GetVariable<string>(VariableNames.TemplateDirectory)!;

            LogInput(() => TemplateDirectory);
            LogInput(() => TemplateName);
            LogInput(() => GeneratedTextKey);

            var text = _textGenerator.GenerateByTemplateName(TemplateDirectory, TemplateName,
                context.GetVariable<object>("Model")!);

            context.Output = text;
            context.SetVariable(GeneratedTextKey, text);
            LogOutput(() => text, $"Length: {text.Length}");

            return Done();
        }
    }
}