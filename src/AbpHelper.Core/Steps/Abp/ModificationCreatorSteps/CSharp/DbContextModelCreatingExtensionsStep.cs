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
        Category = "DbContextModelCreatingExtensionsStep",
        Description = "DbContextModelCreatingExtensionsStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class DbContextModelCreatingExtensionsStep : CSharpModificationCreatorStep
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

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            EntityUsingText ??= context.GetVariable<string>("EntityUsingText")!;
            
            return await base.OnExecuteAsync(context);
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model")!;
            var modelingUsingText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "DbContextModelCreatingExtensions_Using",
                    model);
            var entityConfigText = TextGenerator.GenerateByTemplateName(TemplateDirectory,
                "DbContextModelCreatingExtensions_EntityConfig", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => 1,
                    EntityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(EntityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    modelingUsingText,
                    InsertPosition.After,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(modelingUsingText)),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<MethodDeclarationSyntax>().First().GetEndLine(),
                    entityConfigText,
                    modifyCondition: root =>
                        root.Descendants<MethodDeclarationSyntax>().First().NotContains(entityConfigText)
                )
            };
        }

        public DbContextModelCreatingExtensionsStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}