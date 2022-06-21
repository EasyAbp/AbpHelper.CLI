using System;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Models;
using Elsa.Services.Models;

namespace EasyApp.AbpHelper.Tests.Steps
{
    public class StepTestsBase : AbpHelperTestBase
    {
        protected async Task UsingWorkflowContext(Func<ActivityExecutionContext, Task> action)
        {
            var context =
                new ActivityExecutionContext(ServiceProvider,
                    new WorkflowExecutionContext(ServiceProvider, new WorkflowBlueprint(), new WorkflowInstance()),
                    new ActivityBlueprint(), null, false, CancellationToken.None);
            await action(context);
        }
    }
}