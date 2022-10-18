using System;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class EfCoreConfigurationWorkflow
    {
        public static IActivityBuilder AddEfCoreConfigurationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    /* Add entity property to DbContext class*/
                    .Then<FileFinderStep>(
                        step =>
                        {
                            step.Set(x => x.SearchFileName, x =>
                            {
                                var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                return $"{projectInfo.Name}DbContext.cs";
                            });
                        })
                    .Then<DbContextClassStep>()
                    .Then<FileModifierStep>()
                    .Then<If>(
                        step =>
                        {
                            step.Set(x => x.Condition,
                                x => x.GetVariable<ProjectInfo>("ProjectInfo")!.TemplateType == TemplateType.Module);
                        },
                        ifElse =>
                        {
                            // For module, we also need to modify the IDbContext interface */
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<FileFinderStep>(
                                    step =>
                                    {
                                        step.Set(x => x.SearchFileName, x =>
                                        {
                                            var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                            return $"I{projectInfo.Name}DbContext.cs";
                                        });
                                    })
                                .Then<DbContextInterfaceStep>()
                                .Then<FileModifierStep>()
                                .ThenNamed(ActivityNames.DbContextModel)
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .ThenNamed(ActivityNames.DbContextModel)
                                ;
                        }
                    )
                    /* Add entity configuration to ModelCreating */
                    .Then<FileFinderStep>(step =>
                    {
                        step.Set(x => x.SearchFileName, x =>
                        {
                            var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                            return $"{projectInfo.Name}DbContextModelCreatingExtensions.cs";
                        });
                        step.Set(x => x.ErrorIfNotFound, false);
                    }).WithName(ActivityNames.DbContextModel)
                    .Then<If>(
                        step => { step.Set(x => x.Condition, x => !x.GetInput<string>().IsNullOrWhiteSpace()); },
                        ifElse =>
                        {
                            /* For app using abp v4.4 before version and all versions of the module, using DbContextModelCreatingExtensions by default */
                            ifElse
                                .When(OutcomeNames.True)
                                .ThenNamed("DbContextModelCreating")
                                ;
                            /* For app using abp v4.4 after version, using ProjectNameDbContext by default */
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(step =>
                                {
                                    step.Set(x => x.SearchFileName, x =>
                                    {
                                        var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                        return $"{projectInfo.Name}DbContext.cs";
                                    });
                                })
                                .ThenNamed("DbContextModelCreating")
                                ;
                        }
                    )
                    .Then<DbContextModelCreatingExtensionsStep>().WithName("DbContextModelCreating")
                    .Then<FileModifierStep>()
                ;
        }
    }
}