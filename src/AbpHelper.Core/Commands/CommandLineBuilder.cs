using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyAbp.AbpHelper.Core.Commands
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
            var command = _serviceProvider.GetRequiredService<TCommand>();
            return this.AddCommand(command);
        }
        public CommandLineBuilder AddCommand(Type commandType)
        {
            var command = (Command)_serviceProvider.GetRequiredService(commandType);
            return this.AddCommand(command);
        }
        
        public CommandLineBuilder AddAllRootCommands()
        {
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(CommandBase)))
                .Where(t => t.Namespace!.Split('.').Length == 5) // The namespace of a root command is 5 parts, like "EasyAbp.AbpHelper.Core.Commands.Generate"
                ;
            foreach (var commandType in commandTypes)
            {
                AddCommand(commandType);
            }

            return this;
        }
    }
}