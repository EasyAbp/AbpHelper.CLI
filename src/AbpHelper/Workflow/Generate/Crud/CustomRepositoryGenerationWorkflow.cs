using DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace DosSEdo.AbpHelper.Workflow.Generate.Crud
{
    public static class CustomRepositoryGenerationWorkflow
    {
        public static IActivityBuilder AddCustomRepositoryGeneration(this IOutcomeBuilder builder)
        {
            return builder
                    /* Generate custom repository interface and class */
                    .Then<GroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "Repository"; 
                            step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                        }
                    )
                    /* Add repository configuration to EntityFrameworkCoreModule */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("*EntityFrameworkCoreModule.cs")
                    )
                    .Then<EntityFrameworkCoreModuleStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}