using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AbpHelper.Workflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AbpHelper.Steps
{
    public abstract class StepBase : IStep
    {
        protected readonly string StepName;

        protected StepBase()
        {
            Logger = NullLogger<StepBase>.Instance;
            StepName = GetType().Name;
        }

        public WorkflowContext WorkflowContext { get; set; }

        public ILogger<StepBase> Logger { get; set; }

        public async Task Run()
        {
            Logger.LogDebug($"{StepName} begins.");
            await RunStep();
            Logger.LogDebug($"{StepName} finished.");
        }

        protected abstract Task RunStep();

        protected T GetParameter<T>(string key)
        {
            return (T) WorkflowContext.Parameters[key];
        }

        protected void SetParameter(string key, object value)
        {
            WorkflowContext.Parameters[key] = value;
        }

        protected void LogInput(Expression<Func<object>> parameterExpression)
        {
            LogParameter(parameterExpression, "input");
        }

        protected void LogOutput(Expression<Func<object>> parameterExpression)
        {
            LogParameter(parameterExpression, "output");
        }

        private void LogParameter(Expression<Func<object>> parameterExpression, string parameterType)
        {
            var memberExpr = (MemberExpression) parameterExpression.Body;
            Logger.LogDebug($"{StepName} {parameterType} [{memberExpr.Member.Name}]: {parameterExpression.Compile().Invoke()}");
        }
    }
}