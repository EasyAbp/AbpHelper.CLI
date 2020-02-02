using System;
using System.Linq.Expressions;
using AbpHelper.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace AbpHelper.Workflow
{
    public class StepBuilder<TStep> where TStep : IStep
    {
        private readonly IServiceScope _scope;
        private readonly WorkflowBuilder _workflowBuilder;
        private readonly TStep _step;

        private StepBuilder(WorkflowBuilder workflowBuilder, IServiceScope scope)
        {
            _workflowBuilder = workflowBuilder;
            _scope = scope;
            _step = _scope.ServiceProvider.GetRequiredService<TStep>();
        }

        public static StepBuilder<TStep> CreateBuilder(WorkflowBuilder workflowBuilder, IServiceScope scope)
        {
            return new StepBuilder<TStep>(workflowBuilder, scope);
        }

        public StepBuilder<TStep> WithInput<TInput>(Expression<Func<TStep, TInput>> inputExpression, Expression<Func<WorkflowContext, TInput>> valueExpression)
        {
            var prop = ((MemberExpression) inputExpression.Body).Member;
            var typeParam = Expression.Parameter(typeof(TStep));
            var valueParam = Expression.Parameter(typeof(TInput));
            var lambda = Expression.Lambda<Action<TStep, TInput>>(
                Expression.Assign(
                    Expression.MakeMemberAccess(typeParam, prop),
                    valueParam), typeParam, valueParam);

            var context = _scope.ServiceProvider.GetRequiredService<WorkflowContext>();
            var value = valueExpression.Compile().Invoke(context);
            lambda.Compile()(_step, value);
            return this;
        }

        public WorkflowBuilder DoneAdd()
        {
            _workflowBuilder.Steps.Add(_step);
            return _workflowBuilder;
        }
    }
}