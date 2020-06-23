using DosSEdo.AbpHelper.Steps.Abp;
using DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace DosSEdo.AbpHelper.Workflow.Generate.Crud
{
    public static class UiRazorPagesGenerationWorkflow
    {
        public static IActivityBuilder AddUiRazorPagesGenerationWorkflow(this IOutcomeBuilder builder)
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
                                .Then("UiRazor")
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then("UiRazor")
                                ;
                        }
                    )
                    /* Generate razor pages ui files*/
                    .Then<GroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "UiRazor";
                            step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                        }
                    ).WithName("UiRazor")
                    /* Add menu */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}MenuContributor.cs`")
                    )
                    .Then<MenuContributorStep>()
                    .Then<FileModifierStep>()
                    /* Add mapping */
                    .Then<FileFinderStep>(
                        step =>
                        {
                            step.BaseDirectory = new JavaScriptExpression<string>(@"`${AspNetCoreDir}/src`");
                            step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}WebAutoMapperProfile.cs`");
                        })
                    .Then<WebAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}