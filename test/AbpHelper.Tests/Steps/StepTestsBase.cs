using System;
using System.Threading.Tasks;
using Elsa.Services.Models;

namespace EasyApp.AbpHelper.Tests.Steps
{
    public class StepTestsBase : AbpHelperTestBase
    {
        protected async Task UsingWorkflowContext(Func<WorkflowExecutionContext, Task> action)
        {
            var context = new WorkflowExecutionContext(new Workflow(), null, Application.ServiceProvider);
            await action(context);
        }
    }
}