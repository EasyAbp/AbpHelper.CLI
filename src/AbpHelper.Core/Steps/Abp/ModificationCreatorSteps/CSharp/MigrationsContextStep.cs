using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
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
        Category = "MigrationsContextStep",
        Description = "MigrationsContextStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class MigrationsContextStep : CSharpModificationCreatorStep
    {
        public enum ActionType
        {
            Add,
            Remove
        }

        [ActivityInput(
            Hint = "Action",
            UIHint = ActivityInputUIHints.Dropdown,
            Options = new[] { ActionType.Add, ActionType.Remove },
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public ActionType Action
        {
            get => GetState<ActionType>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "SubmoduleUsingTextPostfix",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? SubmoduleUsingTextPostfix
        {
            get => GetState<string?>();
            set => SetState(value);
        }

        public MigrationsContextStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            SubmoduleUsingTextPostfix ??= context.GetVariable<string>(VariableNames.SubmoduleUsingTextPostfix)!;

            LogInput(() => SubmoduleUsingTextPostfix);
            
            return await base.OnExecuteAsync(context);
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model")! as dynamic;
            model.Bag.SubmoduleUsingTextPostfix = SubmoduleUsingTextPostfix;
            model.Bag.SubmoduleNameText = SubmoduleUsingTextPostfix.Replace(".", "");
            string usingText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "MigrationsContext_Using", model);
            string configText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "MigrationsContext_ConfigureModule", model);

            MethodDeclarationSyntax GetModelCreatingMethod(CSharpSyntaxNode root)
            {
                return root.Descendants<MethodDeclarationSyntax>()
                    .Single(m => m.Identifier.ToString() == "OnModelCreating");
            }

            if (Action == ActionType.Add)
            {
                // Add "builder.ConfigureXXX();"
                return new List<ModificationBuilder<CSharpSyntaxNode>>
                {
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                        usingText,
                        modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(usingText)
                    ),
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => GetModelCreatingMethod(root).GetEndLine(),
                        configText,
                        modifyCondition: root => GetModelCreatingMethod(root).NotContains(configText)
                    ),
                };
            }
            else
            {
                // Remove "builder.ConfigureXXX();"
                Func<CSharpSyntaxNode, int> usingLine = node =>
                    node.Descendants<UsingDirectiveSyntax>().Single(n => n.Contains(usingText)).GetStartLine();
                Func<CSharpSyntaxNode, int> configLine = node =>
                    node.Descendants<ExpressionStatementSyntax>().Last(n => n.Contains(configText)).GetStartLine();
                return new List<ModificationBuilder<CSharpSyntaxNode>>
                {
                    new DeletionBuilder<CSharpSyntaxNode>(
                        root => usingLine(root),
                        root => usingLine(root),
                        modifyCondition: root => root.DescendantsContain<UsingDirectiveSyntax>(usingText)
                    ),
                    new DeletionBuilder<CSharpSyntaxNode>(
                        root => configLine(root),
                        root => configLine(root),
                        modifyCondition: root => root.DescendantsContain<ExpressionStatementSyntax>(configText)
                    ),
                };
            }
        }
    }
}