using System;
using System.Collections.Generic;
using AbpHelper.Models;
using AbpHelper.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace AbpHelper.Workflow
{
    public class WorkflowBuilder
    {
        private readonly IServiceScope _scope;
        private readonly IList<IStep> _steps = new List<IStep>();

        private WorkflowBuilder(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
        }

        public static WorkflowBuilder CreateBuilder(IServiceProvider serviceProvider)
        {
            return new WorkflowBuilder(serviceProvider);
        }

        public WorkflowBuilder AddStep<TStep>() where TStep : IStep
        {
            var step = _scope.ServiceProvider.GetRequiredService<TStep>();
            _steps.Add(step);

            return this;
        }

        public WorkflowBuilder WithParameter(string key, object value)
        {
            var context = _scope.ServiceProvider.GetRequiredService<WorkflowContext>();
            context.Parameters[key] = value;
            return this;
        }

        public Workflow Build()
        {
            var workflow = new Workflow(_steps);
            return workflow;
        }
    }
}