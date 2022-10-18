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
        Category = "EntityFrameworkCoreModuleStep",
        Description = "EntityFrameworkCoreModuleStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class EntityFrameworkCoreModuleStep : CSharpModificationCreatorStep
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
            var contents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "EntityFrameworkCoreModule_AddRepository",
                    context.GetVariable<object>("Model")!);

            CSharpSyntaxNode Func(CSharpSyntaxNode root) => root
                .Descendants<ExpressionStatementSyntax>()
                .First(node => node.ToString().Contains("AddAbpDbContext"))
            ;

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => 1,
                    EntityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(EntityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => Func(root).GetEndLine(),
                    contents,
                    modifyCondition: root => Func(root).NotContains(contents)
                ),
            };
        }

        public EntityFrameworkCoreModuleStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}