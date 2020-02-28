using System;
using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class MenuContributorStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            var projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");
            var model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string addMenuItemContents = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_AddMenuItem", model);

            CSharpSyntaxNode Func(CSharpSyntaxNode root) => root.Descendants<MethodDeclarationSyntax>()
                .Single(n => n.Identifier.ToString().Contains("ConfigureMainMenu"));

            if (projectInfo.TemplateType == TemplateType.Application)
            {
                return new List<ModificationBuilder<CSharpSyntaxNode>>
                {
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => Func(root).GetEndLine(),
                        addMenuItemContents,
                        modifyCondition: root => Func(root).NotContains(addMenuItemContents)
                    )
                };
            }

            if (projectInfo.TemplateType == TemplateType.Module)
            {
                string addUsingContents = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_Using", model);
                string addLocalizerContents = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_Localizer", model);
                return new List<ModificationBuilder<CSharpSyntaxNode>>
                {
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => 2,
                        addUsingContents,
                        modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(addUsingContents)
                    ),
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => Func(root).GetStartLine() + 2,
                        addLocalizerContents,
                        modifyCondition: root => Func(root).NotContains(addLocalizerContents)
                    ),
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => Func(root).GetEndLine() - 1,    // Before `return Task.CompletedTask;`
                        addMenuItemContents,
                        modifyCondition: root => Func(root).NotContains(addMenuItemContents)
                    )
                };
            }

            throw new ArgumentException(projectInfo.TemplateType.ToString(), nameof(projectInfo.TemplateType));
        }
    }
}