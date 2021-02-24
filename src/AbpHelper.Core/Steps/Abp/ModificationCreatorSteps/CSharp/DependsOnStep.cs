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
    public class DependsOnStep : CSharpModificationCreatorStep
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
        
        public DependsOnStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var action = context.EvaluateAsync(Action, CancellationToken.None).GetAwaiter().GetResult();
            var moduleClassNamePostfix = context.GetVariable<string>(VariableNames.ModuleClassNamePostfix);
            var submoduleUsingTextPostfix = context.GetVariable<string>(VariableNames.SubmoduleUsingTextPostfix);
            var dependsOnClassName = context.GetVariable<string>(VariableNames.DependsOnModuleClassName);
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            var model = context.GetVariable<dynamic>("Model");
            model.Bag.ModuleClassNamePostfix = moduleClassNamePostfix;
            model.Bag.DependsOnModuleClassName = dependsOnClassName;
            model.Bag.SubmoduleUsingTextPostfix = submoduleUsingTextPostfix;
            string usingText = TextGenerator.GenerateByTemplateName(templateDir, "ModuleClass_Using", model);
            string dependsOnText = TextGenerator.GenerateByTemplateName(templateDir, "ModuleClass_DependsOn", model);

            if (action == ActionType.Add)
            {
                // Add "[DependsOn(...)]"
                return new List<ModificationBuilder<CSharpSyntaxNode>>
                {
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                        insertPosition: InsertPosition.After,
                        contents: usingText,
                        modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(usingText)
                    ),
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => root.Descendants<ClassDeclarationSyntax>().First().Keyword.GetStartLine(),
                        dependsOnText,
                        modifyCondition: root => root.DescendantsNotContain<ClassDeclarationSyntax>(dependsOnText)
                    ),
                };
            }
            else
            {
                // Delete "[DependsOn(...)]"
                Func<CSharpSyntaxNode, int> usingLine = node => node.Descendants<UsingDirectiveSyntax>().Single(n => n.Contains(usingText)).GetStartLine();
                Func<CSharpSyntaxNode, int> dependsLine = node => node.Descendants<AttributeListSyntax>().Single(n => n.Contains(dependsOnText)).GetStartLine();
                return new List<ModificationBuilder<CSharpSyntaxNode>>
                {
                    new DeletionBuilder<CSharpSyntaxNode>(
                        root => usingLine(root),
                        root => usingLine(root),
                        modifyCondition: root => root.DescendantsContain<UsingDirectiveSyntax>(usingText)
                    ),
                    new DeletionBuilder<CSharpSyntaxNode>(
                        root => dependsLine(root),
                        root => dependsLine(root),
                        modifyCondition: root => root.DescendantsContain<AttributeListSyntax>(dependsOnText)
                    ),
                };
            }
        }
    }
}