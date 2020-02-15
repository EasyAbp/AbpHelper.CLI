using System;
using System.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.AbpHelper.Commands
{
    public abstract class CommandBase : Command, ITransientDependency
    {
        public CommandBase(string name, string? description = null) : base(name, description)
        {
            Logger = NullLogger<CommandBase>.Instance;
        }

        public ILogger<CommandBase> Logger { get; set; }
        public IServiceProvider? ServiceProvider { get; set; }
    }
}