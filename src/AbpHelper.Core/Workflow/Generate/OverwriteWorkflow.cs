using EasyAbp.AbpHelper.Core.Commands;
using EasyAbp.AbpHelper.Core.Commands.Generate;
using Elsa.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate
{
    public static class OverwriteWorkflow
    {
        public static IActivityBuilder AddOverwriteWorkflow(this IActivityBuilder builder)
        {
            return builder
                    .Then<SetVariable>(
                        step =>
                        {
                            step.VariableName = CommandConsts.OverwriteVariableName;
                            step.ValueExpression = new JavaScriptExpression<bool>($"!{CommandConsts.OptionVariableName}.{nameof(GenerateCommandOption.NoOverwrite)}");
                        })
                ;
        }
    }
}