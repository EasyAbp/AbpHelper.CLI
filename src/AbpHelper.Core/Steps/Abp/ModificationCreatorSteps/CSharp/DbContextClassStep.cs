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
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    [Activity(
        Category = "DbContextClassStep",
        Description = "DbContextClassStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class DbContextClassStep : CSharpModificationCreatorStep
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

            LogInput(() => EntityUsingText);

            return await base.OnExecuteAsync(context);
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var dbContextPropertyText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory!, "DbContextClass_Property", context.GetVariable<object>("Model")!);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    EntityUsingText!,
                    InsertPosition.After,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(EntityUsingText!)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().Identifier.GetStartLine() - 1,
                    dbContextPropertyText,
                    modifyCondition: root =>
                        root.DescendantsNotContain<PropertyDeclarationSyntax>(dbContextPropertyText)
                )
            };
        }

        public DbContextClassStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}