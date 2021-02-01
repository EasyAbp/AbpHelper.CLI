using Elsa.Results;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    /// <summary>
    /// This is an empty step.
    /// It can be used as a label, or it can convert `IOutcomeBuilder` to `IActivityBuilder` when writing fluent steps in a workflow,
    /// </summary>
    public class EmptyStep : Step
    {
        protected override ActivityExecutionResult OnExecute(WorkflowExecutionContext context)
        {
            return Done();
        }
    }
}