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
        protected readonly WorkflowContext Context;
        protected readonly string StepName;

        protected StepBase(WorkflowContext context)
        {
            Context = context;
            Logger = NullLogger<StepBase>.Instance;
            StepName = GetType().Name;
        }

        public ILogger<StepBase> Logger { get; set; }

        public async Task Run()
        {
            Logger.LogInformation($"{StepName} begins.");
            await RunStep();
            Logger.LogInformation($"{StepName} finished.");
        }

        protected abstract Task RunStep();

        protected T GetParameter<T>(string key)
        {
            return (T) Context.Parameters[key];
        }

        protected void SetParameter(string key, object value)
        {
            Context.Parameters[key] = value;
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
            Logger.LogInformation($"{StepName} {parameterType} [{memberExpr.Member.Name}]: {parameterExpression.Compile().Invoke()}");
        }
    }
}