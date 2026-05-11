using Elsa.Results;
using Elsa.Services.Models;
using System.Threading;
using System.Threading.Tasks;

namespace EasyAbp.AbpHelper.Core.Steps
{
    public class NullStep : Step
    {
        protected override ActivityExecutionResult OnExecute(WorkflowExecutionContext context)
        {
            return Done();
        }
    }
}