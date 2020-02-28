using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Crud
{
    public static class TestGenerationWorkflow
    {
        public static IActivityBuilder AddTestGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    /* Generate test files */
                    .Then<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "Test";
                            step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                        }
                    )
                ;
        }
    }
}