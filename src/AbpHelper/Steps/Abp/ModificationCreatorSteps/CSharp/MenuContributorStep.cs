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
            string authText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_AuthorizationService", model);
            string addMenuItemText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_AddMenuItem", model);

            CSharpSyntaxNode Func(CSharpSyntaxNode root) => root.Descendants<MethodDeclarationSyntax>()
                .Single(n => n.Identifier.ToString().Contains("ConfigureMainMenu"));

            var builders = new List<ModificationBuilder<CSharpSyntaxNode>>();

            builders.Add(
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => Func(root).GetStartLine() + 2,
                    authText,
                    modifyCondition: root => Func(root).NotContains(authText)
                )
            );

            if (projectInfo.TemplateType == TemplateType.Application)
            {
                string usingForAppText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_UsingForApp", model);
                
                builders.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => 2,
                        usingForAppText,
                        modifyCondition: root => root.NotContains(usingForAppText)
                    ));
                builders.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => Func(root).GetEndLine(),
                        addMenuItemText,
                        modifyCondition: root => Func(root).NotContains(addMenuItemText)
                    )
                );
            }
            else if (projectInfo.TemplateType == TemplateType.Module)
            {
                string usingForModuleText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_UsingForModule", model);
                string localizerText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_Localizer", model);
                builders.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => 2,
                        usingForModuleText,
                        modifyCondition: root => root.NotContains(usingForModuleText)
                    ));
                builders.Add(new InsertionBuilder<CSharpSyntaxNode>(
                    root => Func(root).GetStartLine() + 2,
                    localizerText,
                    modifyCondition: root => Func(root).NotContains(localizerText)
                ));
                builders.Add(new InsertionBuilder<CSharpSyntaxNode>(
                    root => Func(root).GetEndLine() - 1, // Before `return Task.CompletedTask;`
                    addMenuItemText,
                    modifyCondition: root => Func(root).NotContains(addMenuItemText)
                ));
            }

            return builders;
        }
    }
}