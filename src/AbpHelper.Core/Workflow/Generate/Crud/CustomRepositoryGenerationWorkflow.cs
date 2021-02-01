using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
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
                            step.TargetDirectory = new JavaScriptExpression<string>(VariableNames.AspNetCoreDir);
                        }
                    )
                    /* Add repository configuration to EntityFrameworkCoreModule */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}EntityFrameworkCoreModule.cs`")
                    )
                    .Then<EntityFrameworkCoreModuleStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}