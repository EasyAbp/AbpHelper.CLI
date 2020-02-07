using System.Collections.Generic;
using System.Linq;
using AbpHelper.Extensions;
using AbpHelper.Generator;
using AbpHelper.Models;
using AbpHelper.Steps;
using AbpHelper.Steps.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AbpHelper.Workflow.Abp
{
    public static class UIRazorPagesGenerationWorkflow
    {
        public static WorkflowBuilder AddUIRazorPagesGenerationWorkflow(this WorkflowBuilder builder)
        {
            return builder
                    /* Generate razor pages ui files*/
                    .AddStep<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.Model = new
                            {
                                EntityInfo = step.Get<EntityInfo>(),
                                ProjectInfo = step.Get<ProjectInfo>()
                            };
                            step.Overwrite = false;
                            step.GroupName = "UIRazor";
                            step.TargetDirectory = step.GetParameter<string>("BaseDirectory");
                        }
                    )
                    /* Add menu */
                    .AddStep<FileFinderStep>(
                        step => step.SearchFileName = "*MenuContributor.cs"
                    )
                    .AddStep<ModificationCreatorStep>(
                        step =>
                        {
                            var entityInfo = step.Get<EntityInfo>();
                            step.ModificationBuilders = new List<ModificationBuilder>
                            {
                                new InsertionBuilder(root => root.Descendants<MethodDeclarationSyntax>()
                                        .Single(n => n.Identifier.ToString() == "ConfigureMainMenuAsync")
                                        .GetEndLine(),
                                    TextGenerator.GenerateByTemplateName("MenuContributor_AddMenuItem", new {EntityInfo = entityInfo})
                                )
                            };
                        })
                    .AddStep<FileModifierStep>()
                ;
        }
    }
}