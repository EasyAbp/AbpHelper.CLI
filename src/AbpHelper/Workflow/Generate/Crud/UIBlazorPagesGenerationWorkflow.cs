﻿using System.Collections.Generic;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Generate.Crud
{
    public static class UIBlazorPagesGenerationWorkflow
    {
        public static IActivityBuilder AddUIBlazorGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    .Then<IfElse>(
                        step => step.ConditionExpression = new JavaScriptExpression<bool>("ProjectInfo.TemplateType == 1"),
                        ifElse =>
                        {
                            // For module, put generated Razor files under the "ProjectName" folder */
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = "Bag.PagesFolder";
                                        step.ValueExpression = new JavaScriptExpression<string>("ProjectInfo.Name");
                                    }
                                )
                                .Then<SetModelVariableStep>()
                                .Then(ActivityNames.UIBlazor)
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then(ActivityNames.UIBlazor)
                                ;
                        }
                    )
                    /* Generate blazor ui files*/
                    .Then<GroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "UIBlazor";
                            step.TargetDirectory = new JavaScriptExpression<string>(VariableNames.AspNetCoreDir);
                        }
                    ).WithName(ActivityNames.UIBlazor)
                    /* Add menu name */
                    .Then<MultiFileFinderStep>(
                        step =>
                        {
                            step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}Menus.cs`");
                        }
                    )
                    .Then<ForEach>(
                        x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(MultiFileFinderStep.DefaultFileParameterName); },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<MenuNameStep>(step => step.SourceFile = new JavaScriptExpression<string>("CurrentValue"))
                                .Then<FileModifierStep>(step => step.TargetFile = new JavaScriptExpression<string>("CurrentValue"))
                                .Then(branch)
                    )
                    /* Add menu */
                    .Then<MultiFileFinderStep>(
                        step =>
                        {
                            step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}MenuContributor.cs`");
                        }
                    )
                    .Then<ForEach>(
                        x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(MultiFileFinderStep.DefaultFileParameterName); },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<MenuContributorStep>(step => step.SourceFile = new JavaScriptExpression<string>("CurrentValue"))
                                .Then<FileModifierStep>(step => step.TargetFile = new JavaScriptExpression<string>("CurrentValue"))
                                .Then(branch)
                    )
                    /* Add mapping */
                    .Then<FileFinderStep>(
                        step =>
                        {
                            step.BaseDirectory = new JavaScriptExpression<string>(@"`${AspNetCoreDir}/src`");
                            step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}BlazorAutoMapperProfile.cs`");
                        })
                    .Then<BlazorAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}