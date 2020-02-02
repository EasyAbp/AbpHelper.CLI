using System;
using System.Collections.Generic;
using AbpHelper.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace AbpHelper.Workflow
{
    public class WorkflowBuilder
    {
        private readonly IServiceScope _scope;

        private WorkflowBuilder(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
        }

        public IList<IStep> Steps { get; } = new List<IStep>();

        public static WorkflowBuilder CreateBuilder(IServiceProvider serviceProvider)
        {
            return new WorkflowBuilder(serviceProvider);
        }

        public WorkflowBuilder AddStep<TStep>(params Action<TStep>[] actions) where TStep : IStep
        {
            var step = _scope.ServiceProvider.GetRequiredService<TStep>();
            foreach (var action in actions) action(step);
            Steps.Add(step);
            return this;
        }

        public WorkflowBuilder AddStep<TStep>(params Action<TStep, WorkflowContext>[] actions) where TStep : IStep
        {
            var step = _scope.ServiceProvider.GetRequiredService<TStep>();
            var context = _scope.ServiceProvider.GetRequiredService<WorkflowContext>();
            foreach (var action in actions) action(step, context);
            Steps.Add(step);
            return this;
        }

        public Workflow Build()
        {
            var workflow = new Workflow(Steps);
            return workflow;
        }
    }
}