using System;
using System.CommandLine;
using System.CommandLine.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DosSEdo.AbpHelper.Commands
{
    public class CommandLineBuilder : System.CommandLine.Builder.CommandLineBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandLineBuilder(IServiceProvider serviceProvider, string commandName) : base(new RootCommand {Name = commandName})
        {
            _serviceProvider = serviceProvider;
        }

        public CommandLineBuilder AddCommand<TCommand>() where TCommand : CommandBase
        {
            TCommand command = _serviceProvider.GetRequiredService<TCommand>();
            return this.AddCommand(command);
        }
    }
}