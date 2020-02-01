using System.Threading.Tasks;
using AbpHelper.Models;

namespace AbpHelper.Steps
{
    public abstract class StepBase : IStep
    {
        protected readonly WorkflowContext Context;

        protected StepBase(WorkflowContext context)
        {
            Context = context;
        }

        public abstract Task Run();

        protected T GetParameter<T>(string key)
        {
            return (T) Context.Parameters[key];
        }

        protected void SetParameter(string key, object value)
        {
            Context.Parameters[key] = value;
        }
    }
}