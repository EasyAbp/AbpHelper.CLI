using Elsa.Results;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Steps.Common
{
    /// <summary>
    /// This is an empty step.
    /// It can convert `IOutcomeBuilder` to `IActivityBuilder` when writing fluent steps in a workflow,
    /// which is useful when writing extension workflow methods
    /// </summary>
    public class ToActivityBuilder : Step
    {
        protected override ActivityExecutionResult OnExecute(WorkflowExecutionContext context)
        {
            return Done();
        }
    }
}