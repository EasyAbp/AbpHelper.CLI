using System.Collections.Generic;
using System.Linq;
using AbpHelper.Extensions;
using AbpHelper.Generator;
using AbpHelper.Models;
using AbpHelper.Steps;
using AbpHelper.Steps.CSharp;
using Elsa.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AbpHelper.Workflow.Abp
{
    public static class EfCoreConfigurationWorkflow
    {
        public static IActivityBuilder AddEfCoreConfigurationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    /* Add entity property to DbContext */
                    .Then<FileFinderStep>(
                        step =>
                        {
                            step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContext.cs`");
                        })
                    .Then<TextGenerationStep>(
                        step =>
                        {
                            step.TemplateName = "DbContext_Property";
                            step.Model = 
                        }
                    )
                    .Then<ModificationCreatorStep>(
                        step =>
                        {
                            step.ModificationBuilders = new List<ModificationBuilder>
                            {
                                new InsertionBuilder(
                                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                                    new JavaScriptExpression<string>("EntityUsingText"),
                                    InsertPosition.After,
                                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(GetEntityUsingText(step))
                                ),
                                new InsertionBuilder(
                                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().Identifier.GetStartLine() - 1,
                                    TextGenerator.GenerateByTemplateName("DbContext_Property", model)
                                )
                            };
                        })
                    .Then<FileModifierStep>()
                    /* Add entity configuration to DbContextModelCreatingExtensions */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = "*DbContextModelCreatingExtensions.cs"
                    )
                    .Then<ModificationCreatorStep>(
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
                                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(GetEntityUsingText(step))
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
                    .Then<FileModifierStep>()
                ;
        }

        private static string GetEntityUsingText(Step step)
        {
            return step.GetParameter<string>("EntityUsingText");
        }
    }
}