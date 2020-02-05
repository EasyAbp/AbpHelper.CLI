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
        public static WorkflowBuilder AddServiceGenerationWorkflow(this WorkflowBuilder builder)
        {
            return builder
                    .AddStep<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.Model = new
                            {
                                EntityInfo = step.Get<EntityInfo>(),
                                ProjectInfo = step.Get<ProjectInfo>()
                            };
                            step.Overwrite = false;
                            step.GroupName = "Service";
                            step.TargetDirectory = step.GetParameter<string>("BaseDirectory");
                        }
                    )
                    .AddStep<FileFinderStep>(
                        step => step.SearchFileName = "*ApplicationAutoMapperProfile.cs"
                    )
                    .AddStep<ModificationCreatorStep>(
                        step =>
                        {
                            var entityInfo = step.Get<EntityInfo>();
                            step.ModificationBuilders = new List<ModificationBuilder>
                            {
                                new InsertionBuilder(
                                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().GetEndLine(),
                                    TextGenerator.GenerateByTemplateName("CreateMap", new
                                    {
                                        Source = $"{entityInfo.Name}",
                                        Destination = $"{entityInfo.Name}Dto"
                                    })
                                )
                            };
                        })
                    .AddStep<FileModifierStep>()
                ;
        }
    }
}