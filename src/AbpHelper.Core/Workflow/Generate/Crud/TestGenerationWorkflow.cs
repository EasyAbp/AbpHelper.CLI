using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class TestGenerationWorkflow
    {
        public static IActivityBuilder AddTestGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    /* Generate test files */
                    .Then<GroupGenerationStep>(
                        step =>
                        {
                            step.Set(x => x.GroupName, "Test");
                            step.Set(x => x.TargetDirectory, x => x.GetVariable<string>(VariableNames.AspNetCoreDir));
                        }
                    )
                ;
        }
    }
}