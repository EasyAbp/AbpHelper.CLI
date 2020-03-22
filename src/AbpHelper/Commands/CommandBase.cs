using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Elsa.Models;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.AbpHelper.Commands
{
    public abstract class CommandBase : Command, ITransientDependency
    {
        protected readonly IServiceProvider ServiceProvider;

        public ILogger<CommandBase> Logger { get; set; }

        public CommandBase(IServiceProvider serviceProvider, string name, string? description = null) : base(name, description)
        {
            ServiceProvider = serviceProvider;
            Logger = NullLogger<CommandBase>.Instance;
        }

        protected void AddCommand<TCommand>() where TCommand : CommandBase
        {
            var command = ServiceProvider.GetRequiredService<TCommand>();
            base.AddCommand(command);
        }

        protected string GetBaseDirectory(string directory)
        {
            if (directory.IsNullOrEmpty())
            {
                directory = Environment.CurrentDirectory;
            }
            else if (!Directory.Exists(directory))
            {
                Logger.LogError($"Directory '{directory}' does not exist.");
                throw new DirectoryNotFoundException();
            }

            Logger.LogInformation($"Use directory: `{directory}`");

            return directory;
        }

        protected async Task RunWorkflow(Func<IWorkflowBuilder, WorkflowDefinitionVersion> builder)
        {
            var workflowBuilderFactory = ServiceProvider.GetRequiredService<Func<IWorkflowBuilder>>();
            var workflowBuilder = workflowBuilderFactory();

            var workflowDefinition = builder(workflowBuilder);
            // Start the workflow.
            Logger.LogInformation($"Command '{Name}' started.");
            var invoker = ServiceProvider.GetService<IWorkflowInvoker>();
            var ctx = await invoker.StartAsync(workflowDefinition);
            if (ctx.Workflow.Status == WorkflowStatus.Finished)
            {
                Logger.LogInformation($"Command '{Name}' finished successfully.");
            }
        }
    }
}