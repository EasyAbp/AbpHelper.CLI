using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    /// <summary>
    /// This is an empty step.
    /// It can be used as a label, or it can convert `IOutcomeBuilder` to `IActivityBuilder` when writing fluent steps in a workflow,
    /// </summary>
    [Activity(
        Category = "EmptyStep",
        Description = "EmptyStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class EmptyStep : Step
    {
        protected override IActivityExecutionResult OnExecute(ActivityExecutionContext context)
        {
            return Done();
        }
    }
}