using EasyAbp.AbpHelper.Core.Commands;
using EasyAbp.AbpHelper.Core.Commands.Generate;
using Elsa.Activities.Primitives;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate
{
    public static class OverwriteWorkflow
    {
        public static IActivityBuilder AddOverwriteWorkflow(this IActivityBuilder builder, GenerateCommandOption option)
        {
            return builder
                    .Then<SetVariable>(step =>
                    {
                        step.Set(x => x.VariableName, CommandConsts.OverwriteVariableName);
                        step.Set(x => x.Value, option.NoOverwrite);
                    })
                ;
        }
    }
}