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
    public static class EfCoreConfigurationWorkflow
    {
        public static WorkflowBuilder AddEfCoreConfigurationWorkflow(this WorkflowBuilder builder)
        {
            return builder
                    /* Add entity property to DbContext */
                    .AddStep<FileFinderStep>(
                        step => { step.SearchFileName = $"{step.GetParameter<ProjectInfo>("ProjectInfo").Name}DbContext.cs"; })
                    .AddStep<ModificationCreatorStep>(
                        step =>
                        {
                            var model = new {EntityInfo = GetEntityInfo(step)};
                            step.ModificationBuilders = new List<ModificationBuilder>
                            {
                                new InsertionBuilder(
                                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                                    GetEntityUsingText(step),
                                    InsertPosition.After,
                                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(GetEntityUsingText(step))
                                ),
                                new InsertionBuilder(
                                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().Identifier.GetStartLine() - 1,
                                    TextGenerator.Generate("DbContext_Property", model)
                                )
                            };
                        })
                    .AddStep<FileModifierStep>(
                        step => step.Modifications = step.GetParameter<IList<Modification>>("Modifications")
                    )
                    /* Add entity configuration to DbContextModelCreatingExtensions */
                    .AddStep<FileFinderStep>(
                        step => step.SearchFileName = "*DbContextModelCreatingExtensions.cs"
                    )
                    .AddStep<ModificationCreatorStep>(
                        step =>
                        {
                            var model = new
                            {
                                EntityInfo = GetEntityInfo(step),
                                ProjectInfo = step.GetParameter<ProjectInfo>("ProjectInfo")
                            };
                            var modelingUsingText = TextGenerator.Generate("DbContextModelCreatingExtensions_Using", model);
                            var entityConfigText = TextGenerator.Generate("DbContextModelCreatingExtensions_EntityConfig", model);
                            step.ModificationBuilders = new List<ModificationBuilder>
                            {
                                new InsertionBuilder(
                                    root => 1,
                                    GetEntityUsingText(step),
                                    shouldModifier: root => root.DescendantsNotContain<UsingDirectiveSyntax>(GetEntityUsingText(step))
                                ),
                                new InsertionBuilder(
                                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                                    modelingUsingText,
                                    InsertPosition.After,
                                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(modelingUsingText)),
                                new InsertionBuilder(root => root.Descendants<MethodDeclarationSyntax>().First().GetEndLine(),
                                    entityConfigText
                                )
                            };
                        })
                    .AddStep<FileModifierStep>(
                        step => step.Modifications = step.GetParameter<IList<Modification>>("Modifications")
                    )
                ;
        }

        private static EntityInfo GetEntityInfo(Step step)
        {
            return step.GetParameter<EntityInfo>("EntityInfo");
        }

        private static ProjectInfo GetProjectInfo(Step step)
        {
            return step.GetParameter<ProjectInfo>("ProjectInfo");
        }

        private static string GetEntityUsingText(Step step)
        {
            return step.GetParameter<string>("EntityUsingText");
        }
    }
}