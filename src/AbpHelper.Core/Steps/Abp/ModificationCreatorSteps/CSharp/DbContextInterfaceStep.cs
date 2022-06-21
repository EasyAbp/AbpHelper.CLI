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
        Category = "DbContextInterfaceStep",
        Description = "DbContextInterfaceStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class DbContextInterfaceStep : CSharpModificationCreatorStep
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
            var dbContextUsingText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "DbContextInterface_Using", model);
            var dbContextPropertyText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "DbContextInterface_Property", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => 1,
                    dbContextUsingText,
                    InsertPosition.Before,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(dbContextUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    EntityUsingText,
                    InsertPosition.After,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(EntityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<InterfaceDeclarationSyntax>().Single().GetEndLine(),
                    dbContextPropertyText,
                    modifyCondition: root =>
                        root.DescendantsNotContain<PropertyDeclarationSyntax>(dbContextPropertyText)
                )
            };
        }

        public DbContextInterfaceStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}