using System.Collections.Generic;
using EasyAbp.AbpHelper.Core.Commands;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class UiRazorPagesGenerationWorkflow
    {
        public static IActivityBuilder AddUiRazorPagesGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    .Then<If>(
                        step =>
                        {
                            step.Set(x => x.Condition,
                                x => x.GetVariable<ProjectInfo>("ProjectInfo")!.TemplateType == TemplateType.Module);
                        },
                        ifElse =>
                        {
                            // For module, put generated Razor files under the "ProjectName" folder */
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, "Bag.PagesFolder");
                                    step.Set(x => x.Value, x => x.GetVariable<ProjectInfo>("ProjectInfo")!.Name);
                                })
                                .Then<SetModelVariableStep>()
                                .ThenNamed(ActivityNames.UiRazor)
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .ThenNamed(ActivityNames.UiRazor)
                                ;
                        }
                    )
                    /* Generate razor pages ui files*/
                    .Then<GroupGenerationStep>(
                        step =>
                        {
                            step.Set(x => x.GroupName, "UiRazor");
                            step.Set(x => x.TargetDirectory, x => x.GetVariable<string>(VariableNames.AspNetCoreDir));
                        }
                    ).WithName(ActivityNames.UiRazor)
                    /* Add menu name */
                    .Then<MultiFileFinderStep>(
                        step =>
                        {
                            step.Set(x => x.SearchFileName, x =>
                            {
                                var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                return $"{projectInfo.Name}Menus.cs";
                            });
                        }
                    )
                    .Then<ForEach>(
                        step =>
                        {
                            step.Set(x => x.Items,
                                x => x.GetVariable<IList<object>>(MultiFileFinderStep.DefaultFileParameterName));
                        },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<MenuNameStep>(step => { step.Set(x => x.SourceFile, x => x.GetInput<string>()); })
                                .Then<FileModifierStep>(step =>
                                {
                                    step.Set(x => x.TargetFile, x => x.GetInput<string>());
                                })
                    )
                    /* Add menu */
                    .Then<MultiFileFinderStep>(
                        step =>
                        {
                            step.Set(x => x.BaseDirectory, x =>
                            {
                                var aspNetCoreDir = x.GetVariable<string>(VariableNames.AspNetCoreDir);
                                return $"{aspNetCoreDir}/src";
                            });
                            step.Set(x => x.SearchFileName, x =>
                            {
                                var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                return $"{projectInfo.Name}MenuContributor.cs";
                            });
                            step.Set(x => x.ExcludeDirectories,
                                x => x.GetVariable<string[]>(CommandConsts.ExcludeDirectoriesVariableName));
                        }
                    )
                    .Then<ForEach>(
                        step =>
                        {
                            step.Set(x => x.Items,
                                x => x.GetVariable<IList<object>>(MultiFileFinderStep.DefaultFileParameterName));
                        },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<MenuContributorStep>(step =>
                                {
                                    step.Set(x => x.SourceFile, x => x.GetInput<string>());
                                })
                                .Then<FileModifierStep>(step =>
                                {
                                    step.Set(x => x.TargetFile, x => x.GetInput<string>());
                                })
                    )
                    /* Add mapping */
                    .Then<FileFinderStep>(
                        step =>
                        {
                            step.Set(x => x.BaseDirectory, x =>
                            {
                                var aspNetCoreDir = x.GetVariable<string>(VariableNames.AspNetCoreDir);
                                return $"{aspNetCoreDir}/src";
                            });
                            step.Set(x => x.SearchFileName, x =>
                            {
                                var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                return $"{projectInfo.Name}WebAutoMapperProfile.cs";
                            });
                            step.Set(x => x.ExcludeDirectories,
                                x => x.GetVariable<string[]>(CommandConsts.ExcludeDirectoriesVariableName));
                        })
                    .Then<WebAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}