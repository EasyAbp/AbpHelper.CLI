﻿using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class MenuContributorStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");
            var model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            string addMenuItemText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_AddMenuItem", model);

            CSharpSyntaxNode MainMenu(CSharpSyntaxNode root) => root.Descendants<MethodDeclarationSyntax>()
                .First(n => n.Identifier.ToString().Contains("ConfigureMainMenu"));

            var builders = new List<ModificationBuilder<CSharpSyntaxNode>>();

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
                        root => MainMenu(root).GetEndLine(),
                        addMenuItemText,
                        modifyCondition: root => MainMenu(root).NotContains(addMenuItemText)
                    )
                );
            }
            else if (projectInfo.TemplateType == TemplateType.Module)
            {
                string configureMainMenuText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_ConfigureMainMenu", model);
                string usingForModuleText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_UsingForModule", model);
                string localizerText = TextGenerator.GenerateByTemplateName(templateDir, "MenuContributor_Localizer", model);
                builders.Add(
                    new ReplacementBuilder<CSharpSyntaxNode>(
                        root => MainMenu(root).GetStartLine(),
                        root => MainMenu(root).GetStartLine(),
                        configureMainMenuText,
                        modifyCondition: root => root.NotContains(configureMainMenuText)
                    ));
                builders.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => 2,
                        usingForModuleText,
                        modifyCondition: root => root.NotContains(usingForModuleText)
                    ));
                builders.Add(new InsertionBuilder<CSharpSyntaxNode>(
                    root => MainMenu(root).GetStartLine() + 2,
                    localizerText,
                    modifyCondition: root => MainMenu(root).NotContains(localizerText)
                ));
                builders.Add(new DeletionBuilder<CSharpSyntaxNode>(
                    root => MainMenu(root).GetEndLine() - 1,
                    root => MainMenu(root).GetEndLine() - 1,
                    modifyCondition: root => !root.NotContains("return Task.CompletedTask;")
                ));
                builders.Add(new InsertionBuilder<CSharpSyntaxNode>(
                    root => MainMenu(root).GetEndLine(),
                    addMenuItemText,
                    modifyCondition: root => MainMenu(root).NotContains(addMenuItemText)
                ));
            }

            return builders;
        }

        public MenuContributorStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}