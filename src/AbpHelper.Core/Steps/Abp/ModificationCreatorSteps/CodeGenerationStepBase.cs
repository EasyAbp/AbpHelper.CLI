using EasyAbp.AbpHelper.Core.Generator;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps;

public abstract class CodeGenerationStepBase : Step
{
    protected readonly TextGenerator TextGenerator;

    protected CodeGenerationStepBase(TextGenerator textGenerator)
    {
        TextGenerator = textGenerator;
    }

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
}