using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.Expressions;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class MigrationsContextStep : CSharpModificationCreatorStep
    {        
        public enum ActionType
        {
            Add, Remove
        }
        public WorkflowExpression<ActionType> Action
        {
            get => GetState<WorkflowExpression<ActionType>>();
            set => SetState(value);
        }
        public MigrationsContextStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var action = context.EvaluateAsync(Action, CancellationToken.None).GetAwaiter().GetResult();
            var submoduleUsingTextPostfix = context.GetVariable<string>(VariableNames.SubmoduleUsingTextPostfix);
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            var model = context.GetVariable<dynamic>("Model");
            model.Bag.SubmoduleUsingTextPostfix = submoduleUsingTextPostfix;
            model.Bag.SubmoduleNameText = submoduleUsingTextPostfix.Replace(".", "");
            string usingText = TextGenerator.GenerateByTemplateName(templateDir, "MigrationsContext_Using", model);
            string configText = TextGenerator.GenerateByTemplateName(templateDir, "MigrationsContext_ConfigureModule", model);

            MethodDeclarationSyntax GetModelCreatingMethod(CSharpSyntaxNode root)
            {
                return root.Descendants<MethodDeclarationSyntax>().Single(m => m.Identifier.ToString() == "OnModelCreating");
            }

            if (action == ActionType.Add)
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
                Func<CSharpSyntaxNode, int> usingLine = node => node.Descendants<UsingDirectiveSyntax>().Single(n => n.Contains(usingText)).GetStartLine();
                Func<CSharpSyntaxNode, int> configLine = node => node.Descendants<ExpressionStatementSyntax>().Last(n => n.Contains(configText)).GetStartLine();
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