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
    public static class ServiceGenerationWorkflow
    {
        public static IActivityBuilder AddServiceGenerationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    /* Generate dto, service interface and class files */
                    .Then<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.Model = new
                            {
                                EntityInfo = step.Get<EntityInfo>(),
                                ProjectInfo = step.Get<ProjectInfo>()
                            };
                            step.GroupName = "Service";
                            step.TargetDirectory = step.GetParameter<string>("BaseDirectory");
                        }
                    )
                    /* Add mapping */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = "*ApplicationAutoMapperProfile.cs"
                    )
                    .Then<ModificationCreatorStep>(
                        step =>
                        {
                            var entityInfo = step.Get<EntityInfo>();
                            step.ModificationBuilders = new List<ModificationBuilder>
                            {
                                new InsertionBuilder(
                                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                                    GetEntityUsingText(step),
                                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(GetEntityUsingText(step))
                                ),
                                new InsertionBuilder(
                                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                                    GetEntityDtoUsingText(step),
                                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(GetEntityDtoUsingText(step))
                                ),
                                new InsertionBuilder(
                                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().GetEndLine(),
                                    TextGenerator.GenerateByTemplateName("ApplicationAutoMapperProfile_CreateMap", new {EntityInfo = entityInfo})
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

        private static string GetEntityDtoUsingText(Step step)
        {
            return step.GetParameter<string>("EntityDtoUsingText");
        }
    }
}