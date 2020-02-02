using System.Collections.Generic;

namespace AbpHelper.Workflow
{
    public class WorkflowContext
    {
        private readonly IDictionary<string, object> _parameters;

        public WorkflowContext(IDictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public T GetParameter<T>(string key)
        {
            return (T) _parameters[key];
        }

        public void SetParameter(string key, object value)
        {
            _parameters[key] = value;
        }
    }
}