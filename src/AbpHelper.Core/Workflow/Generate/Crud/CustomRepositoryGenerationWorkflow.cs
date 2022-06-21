using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Builders;

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
                            step.Set(x => x.GroupName, "Repository");
                            step.Set(x => x.TargetDirectory, x => x.GetVariable<string>(VariableNames.AspNetCoreDir));
                        }
                    )
                    /* Add repository configuration to EntityFrameworkCoreModule */
                    .Then<FileFinderStep>(
                        step =>
                        {
                            step.Set(x => x.SearchFileName, x =>
                            {
                                var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                return $"*{projectInfo.Name}EntityFrameworkCoreModule.cs";
                            });
                        })
                    .Then<EntityFrameworkCoreModuleStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}