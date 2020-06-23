using System;
using System.Linq.Expressions;
using Elsa.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DosSEdo.AbpHelper.Steps
{
    public abstract class Step : Activity
    {
        protected Step()
        {
            Logger = NullLogger<Step>.Instance;
        }

        public ILogger<Step> Logger { get; set; }

        protected void LogInput<TParameter>(Expression<Func<TParameter>> parameterExpression, object? customValue = null)
        {
            LogParameter("Input", parameterExpression, customValue);
        }

        protected void LogOutput<TParameter>(Expression<Func<TParameter>> parameterExpression, object? customValue = null)
        {
            LogParameter("Output", parameterExpression, customValue);
        }

        private void LogParameter<TParameter>(string parameterType, Expression<Func<TParameter>> parameterExpression, object? customValue = null)
        {
            MemberExpression memberExpr = (MemberExpression) parameterExpression.Body;
            object value = customValue ?? $"'{parameterExpression.Compile().Invoke()}'";
            Logger.LogDebug($"{Type} {parameterType} [{memberExpr.Member.Name}]: {value}");
        }
    }
}