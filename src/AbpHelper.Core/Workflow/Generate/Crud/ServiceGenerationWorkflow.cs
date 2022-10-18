using EasyAbp.AbpHelper.Core.Commands.Generate.Crud;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class ServiceGenerationWorkflow
    {
        public static IActivityBuilder AddServiceGenerationWorkflow(this IActivityBuilder builder, string name,
            CrudCommandOption option)
        {
            return builder
                    /* Generate dto, service interface and class files */
                    .Then<GroupGenerationStep>(
                        step =>
                        {
                            step.Set(x => x.GroupName, "Service");
                            step.Set(x => x.TargetDirectory, x => x.GetVariable<string>(VariableNames.AspNetCoreDir));
                        }
                    ).WithName(name)
                    /* Generate permissions */
                    .Then<If>(
                        ifElse => { ifElse.Set(x => x.Condition, option.SkipPermissions); },
                        ifElse =>
                        {
                            ifElse
                                .When(OutcomeNames.True)
                                .ThenNamed(ActivityNames.AutoMapper)
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(step =>
                                {
                                    step.Set(x => x.SearchFileName, x =>
                                    {
                                        var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                        return $"{projectInfo.Name}Permissions.cs";
                                    });
                                })
                                .Then<PermissionsStep>()
                                .Then<FileModifierStep>()
                                .Then<FileFinderStep>(step =>
                                {
                                    step.Set(x => x.SearchFileName, x =>
                                    {
                                        var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                        return $"{projectInfo.Name}PermissionDefinitionProvider.cs";
                                    });
                                })
                                .Then<PermissionDefinitionProviderStep>()
                                .Then<FileModifierStep>()
                                .ThenNamed(ActivityNames.AutoMapper)
                                ;
                        }
                    )
                    /* Add mapping */
                    .Then<FileFinderStep>(step =>
                    {
                        step.Set(x => x.SearchFileName, x =>
                        {
                            var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                            return $"{projectInfo.Name}ApplicationAutoMapperProfile.cs";
                        });
                    })
                    .WithName(ActivityNames.AutoMapper)
                    .Then<ApplicationAutoMapperProfileStep>(
                        step =>
                        {
                            step.Set(x => x.EntityUsingText, x => x.GetVariable<string>("EntityUsingText"));
                            step.Set(x => x.EntityDtoUsingText, x => x.GetVariable<string>("EntityDtoUsingText"));
                        })
                    .Then<FileModifierStep>()
                ;
        }
    }
}