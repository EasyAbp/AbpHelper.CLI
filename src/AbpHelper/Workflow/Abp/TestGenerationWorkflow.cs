using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class TestGenerationWorkflow
    {
        public static IActivityBuilder AddTestGenerationWorkflow(this IActivityBuilder builder, string name)
        {
            return builder
                    /* Generate test files */
                    .Then<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "Test";
                            step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                        }
                    ).WithName(name)
                ;
        }
    }
}