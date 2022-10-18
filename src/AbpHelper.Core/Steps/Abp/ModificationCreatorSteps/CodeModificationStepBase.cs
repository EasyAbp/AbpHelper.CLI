using EasyAbp.AbpHelper.Core.Generator;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps;

public abstract class CodeModificationStepBase : CodeGenerationStepBase
{
    [ActivityInput(
        Hint = "SourceFile",
        UIHint = ActivityInputUIHints.SingleLine,
        SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
    )]
    public string? SourceFile
    {
        get => GetState<string?>();
        set => SetState(value);
    }

    protected CodeModificationStepBase(TextGenerator textGenerator) : base(textGenerator)
    {
    }
}