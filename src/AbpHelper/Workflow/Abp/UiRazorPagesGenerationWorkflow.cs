using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class UiRazorPagesGenerationWorkflow
    {
        public static IActivityBuilder AddUiRazorPagesGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    /* Generate razor pages ui files*/
                    .Then<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "UiRazor";
                            step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                        }
                    )
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
                            step.BaseDirectory = new JavaScriptExpression<string>("`${BaseDirectory}/src`");
                            step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}WebAutoMapperProfile.cs`");
                        })
                    .Then<WebAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}