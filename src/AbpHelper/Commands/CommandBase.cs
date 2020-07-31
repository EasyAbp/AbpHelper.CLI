using System;
using System.CommandLine;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace EasyAbp.AbpHelper.Commands
{
    public abstract class CommandBase : Command, ITransientDependency
    {
        protected CommandBase(IServiceProvider serviceProvider, string name, string? description) : base(name, description)
        {
            ServiceProvider = serviceProvider;
        }

        protected readonly IServiceProvider ServiceProvider;

        protected void AddCommand<TCommand>() where TCommand : CommandBase
        {
            var command = ServiceProvider.GetRequiredService<TCommand>();
            AddCommand(command);
        }
    }
}