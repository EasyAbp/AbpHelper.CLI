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

        public IList<Step> Steps { get; } = new List<Step>();

        public static WorkflowBuilder CreateBuilder(IServiceProvider serviceProvider)
        {
            return new WorkflowBuilder(serviceProvider);
        }

        public WorkflowBuilder AddStep<TStep>(params Action<TStep>[] actions) where TStep : Step
        {
            var step = _scope.ServiceProvider.GetRequiredService<TStep>();
            foreach (var action in actions) step.AddInputAction(action);
            Steps.Add(step);
            return this;
        }

        public Workflow Build()
        {
            var workflow = new Workflow(Steps);
            return workflow;
        }

        public WorkflowBuilder WithParameter(string key, object value)
        {
            var context = _scope.ServiceProvider.GetRequiredService<WorkflowContext>();
            context.SetParameter(key, value);
            return this;
        }
    }
}