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

        protected WorkflowContext WorkflowContext;

        protected StepBase(WorkflowContext workflowContext)
        {
            StepName = GetType().Name;
            WorkflowContext = workflowContext;
            Logger = NullLogger<StepBase>.Instance;
        }

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

        protected void LogInput<TParameter>(Expression<Func<TParameter>> parameterExpression, object? customValue = null)
        {
            LogParameter("input", parameterExpression, customValue);
        }

        protected void LogOutput<TParameter>(Expression<Func<TParameter>> parameterExpression, object? customValue = null)
        {
            LogParameter("output", parameterExpression, customValue);
        }

        private void LogParameter<TParameter>(string parameterType, Expression<Func<TParameter>> parameterExpression, object? customValue = null)
        {
            var memberExpr = (MemberExpression) parameterExpression.Body;
            var value = customValue ?? parameterExpression.Compile().Invoke();
            Logger.LogDebug($"{StepName} {parameterType} [{memberExpr.Member.Name}]: {value}");
        }
    }
}