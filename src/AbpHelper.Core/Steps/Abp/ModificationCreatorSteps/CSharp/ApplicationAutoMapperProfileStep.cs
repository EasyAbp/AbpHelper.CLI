using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    [Activity(
        Category = "ApplicationAutoMapperProfileStep",
        Description = "ApplicationAutoMapperProfileStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class ApplicationAutoMapperProfileStep : CSharpModificationCreatorStep
    {
        [ActivityInput(
            Hint = "EntityUsingText",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? EntityUsingText
        {
            get => GetState<string?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "EntityDtoUsingText",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? EntityDtoUsingText
        {
            get => GetState<string?>();
            set => SetState(value);
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            EntityUsingText ??= context.GetVariable<string>("EntityUsingText")!;
            EntityDtoUsingText ??= context.GetVariable<string>("EntityDtoUsingText")!;

            return await base.OnExecuteAsync(context);
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var contents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "ApplicationAutoMapperProfile_CreateMap",
                    context.GetVariable<object>("Model")!);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    EntityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(EntityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    EntityDtoUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(EntityDtoUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().GetEndLine(),
                    contents,
                    modifyCondition: root =>
                        root.Descendants<ConstructorDeclarationSyntax>().Single().NotContains(contents)
                )
            };
        }

        public ApplicationAutoMapperProfileStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}