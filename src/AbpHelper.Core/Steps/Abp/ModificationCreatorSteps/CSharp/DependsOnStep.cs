using System;
using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
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
        Category = "DependsOnStep",
        Description = "DependsOnStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class DependsOnStep : CSharpModificationCreatorStep
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
            Hint = "ModuleClassNamePostfix",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string ModuleClassNamePostfix
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "SubmoduleUsingTextPostfix",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string SubmoduleUsingTextPostfix
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "DependsOnModuleClassName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string DependsOnModuleClassName
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        public DependsOnStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model")! as dynamic;
            model.Bag.ModuleClassNamePostfix = ModuleClassNamePostfix;
            model.Bag.DependsOnModuleClassName = DependsOnModuleClassName;
            model.Bag.SubmoduleUsingTextPostfix = SubmoduleUsingTextPostfix;
            string usingText = TextGenerator.GenerateByTemplateName(TemplateDirectory, "ModuleClass_Using", model);
            string dependsOnText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "ModuleClass_DependsOn", model);

            if (Action == ActionType.Add)
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
                Func<CSharpSyntaxNode, int> usingLine = node =>
                    node.Descendants<UsingDirectiveSyntax>().Single(n => n.Contains(usingText)).GetStartLine();
                Func<CSharpSyntaxNode, int> dependsLine = node =>
                    node.Descendants<AttributeListSyntax>().Single(n => n.Contains(dependsOnText)).GetStartLine();
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