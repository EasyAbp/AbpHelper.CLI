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
                        step => { step.SearchFileName = $"{step.Get<ProjectInfo>().Name}DbContext.cs"; })
                    .AddStep<ModificationCreatorStep>(
                        step =>
                        {
                            var model = new {EntityInfo = step.Get<EntityInfo>()};
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
                                    TextGenerator.GenerateByTemplateName("DbContext_Property", model)
                                )
                            };
                        })
                    .AddStep<FileModifierStep>()
                    /* Add entity configuration to DbContextModelCreatingExtensions */
                    .AddStep<FileFinderStep>(
                        step => step.SearchFileName = "*DbContextModelCreatingExtensions.cs"
                    )
                    .AddStep<ModificationCreatorStep>(
                        step =>
                        {
                            var model = new
                            {
                                EntityInfo = step.Get<EntityInfo>(),
                                ProjectInfo = step.Get<ProjectInfo>()
                            };
                            var modelingUsingText = TextGenerator.GenerateByTemplateName("DbContextModelCreatingExtensions_Using", model);
                            var entityConfigText = TextGenerator.GenerateByTemplateName("DbContextModelCreatingExtensions_EntityConfig", model);
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
                    .AddStep<FileModifierStep>()
                ;
        }

        private static string GetEntityUsingText(Step step)
        {
            return step.GetParameter<string>("EntityUsingText");
        }
    }
}