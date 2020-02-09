using AbpHelper.Steps.Abp.ModificationCreatorSteps;
using AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace AbpHelper.Workflow.Abp
{
    public static class UIRazorPagesGenerationWorkflow
    {
        public static IActivityBuilder AddUIRazorPagesGenerationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    /* Generate razor pages ui files*/
                    .Then<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "UIRazor";
                            step.TargetDirectory = new JavaScriptExpression<string>("BaseDirectory");
                        }
                    )
                    /* Add menu */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("*MenuContributor.cs")
                    )
                    .Then<MenuContributorStep>()
                    .Then<FileModifierStep>()
                    /* Add mapping */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression<string>("*WebAutoMapperProfile.cs")
                    )
                    .Then<WebAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                /*
                /* Add localization #1#
                .Then<FileFinderStep>(
                    step => step.Multiple = true,
                    step => step.SearchFileName = "*.json",
                    step =>
                    {
                        var projectInfo = step.Get<ProjectInfo>();
                        step.BaseDirectory = $@"{projectInfo.BaseDirectory}\src\{projectInfo.FullName}.Domain.Shared\Localization";
                    }
                )
            */
                /*
                .Then<LoopStep<string>>(
                    step => step.LoopOn = () => step.GetParameter<string[]>(FileFinderStep.DefaultFilesParameterName),
                    step => step.LoopBody = file =>
                    {
                        
                    }
                    )
                */
                ;
        }
    }
}