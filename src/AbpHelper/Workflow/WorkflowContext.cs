using System.Collections.Generic;

namespace AbpHelper.Workflow
{
    public class WorkflowContext
    {
        public WorkflowContext(IDictionary<string, object> parameters)
        {
            Parameters = parameters;
        }

        public IDictionary<string, object> Parameters { get; }
    }
}