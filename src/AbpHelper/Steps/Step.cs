using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AbpHelper.Workflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace AbpHelper.Steps
{
    public abstract class Step : ITransientDependency
    {
        private readonly IList<Action<object>> _inputActions = new List<Action<object>>();
        protected readonly string StepName;

        protected Step()
        {
            StepName = GetType().Name;
            Logger = NullLogger<Step>.Instance;
            Context = new WorkflowContext(new Dictionary<string, object>());
        }

        public WorkflowContext Context { get; set; }

        public ILogger<Step> Logger { get; set; }

        public async Task Run()
        {
            Logger.LogDebug($"{StepName} begins.");
            foreach (var inputAction in _inputActions) inputAction(this);
            await RunStep();
            Logger.LogDebug($"{StepName} finished.");
        }

        public bool ContainsParameter(string key)
        {
            return Context.ContainsParameter(key);
        }

        public T GetParameter<T>(string key)
        {
            return Context.GetParameter<T>(key);
        }

        public void SetParameter(string key, object value)
        {
            Context.SetParameter(key, value);
        }

        protected abstract Task RunStep();

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

        public void AddInputAction<TStep>(Action<TStep> action) where TStep : Step
        {
            _inputActions.Add(o => action((TStep) o));
        }
    }
}