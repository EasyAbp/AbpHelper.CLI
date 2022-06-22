using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Attributes;
using EasyAbp.AbpHelper.Core.Services;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using Elsa.Activities.Primitives;
using Elsa.Builders;
using Elsa.Models;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyAbp.AbpHelper.Core.Commands
{
    public abstract class CommandWithOption<TOption> : CommandBase where TOption : CommandOptionsBase
    {
        public CommandWithOption(IServiceProvider serviceProvider, string name, string? description = null) : base(
            serviceProvider, name, description)
        {
            Logger = NullLogger<CommandWithOption<TOption>>.Instance;

            AddArgumentAndOptions();

            Handler = CommandHandler.Create((TOption option) => RunCommand(option));
        }

        protected virtual string OptionVariableName => CommandConsts.OptionVariableName;
        protected virtual string BaseDirectoryVariableName => CommandConsts.BaseDirectoryVariableName;
        protected virtual string ExcludeDirectoriesVariableName => CommandConsts.ExcludeDirectoriesVariableName;
        protected virtual string OverwriteVariableName => CommandConsts.OverwriteVariableName;

        public ILogger<CommandWithOption<TOption>> Logger { get; set; }

        public virtual async Task RunCommand(TOption option)
        {
            option.Directory = GetBaseDirectory(option.Directory);

            await ServiceProvider.GetRequiredService<ICheckUpdateService>().CheckUpdateAsync();

            await RunWorkflow(builder =>
            {
                var activityBuilder = builder
                        .StartWith<SetVariable>(
                            step =>
                            {
                                step.Set(x => x.VariableName, OptionVariableName);
                                step.Set(x => x.Value, option);
                            })
                        .Then<SetVariable>(
                            step =>
                            {
                                step.Set(x => x.VariableName, BaseDirectoryVariableName);
                                step.Set(x => x.Value, option.Directory);
                            })
                        .Then<SetVariable>(
                            step =>
                            {
                                step.Set(x => x.VariableName, ExcludeDirectoriesVariableName);
                                step.Set(x => x.Value, option.Exclude);
                            })
                        .Then<ProjectInfoProviderStep>(step =>
                        {
                            step.Set(x => x.ExcludeDirectories,
                                x => x.GetVariable<string[]>(CommandConsts.ExcludeDirectoriesVariableName));
                            step.Set(x => x.Overwrite, x => x.GetVariable<bool>(CommandConsts.OverwriteVariableName));
                        })
                    ;

                return ConfigureBuild(option, activityBuilder).Build();
            });
        }

        protected virtual IActivityBuilder ConfigureBuild(TOption option, IActivityBuilder activityBuilder)
        {
            return activityBuilder;
        }

        protected virtual string GetBaseDirectory(string directory)
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

        protected async Task RunWorkflow(Func<IWorkflowBuilder, IWorkflowBlueprint> builder)
        {
            var workflowBuilderFactory = ServiceProvider.GetRequiredService<Func<IWorkflowBuilder>>();
            var workflowBuilder = workflowBuilderFactory();

            var workflowDefinition = builder(workflowBuilder);
            // Start the workflow.
            Logger.LogInformation($"Command '{Name}' started.");
            var invoker = ServiceProvider.GetRequiredService<IStartsWorkflow>();
            var ctx = await invoker.StartWorkflowAsync(workflowDefinition);
            if (ctx.WorkflowInstance?.WorkflowStatus == WorkflowStatus.Finished)
            {
                Logger.LogInformation($"Command '{Name}' finished successfully.");
            }
            else
            {
                Logger.LogError("Error activity: " + ctx.ActivityId);
            }
        }

        private void AddArgumentAndOptions()
        {
            foreach (var propertyInfo in typeof(TOption).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var optionAttribute = propertyInfo.GetCustomAttribute<OptionAttribute>();
                if (optionAttribute != null)
                {
                    var alias = new List<string>();
                    if (!optionAttribute.ShortName.IsNullOrWhiteSpace())
                    {
                        alias.Add("-" + optionAttribute.ShortName);
                    }

                    if (!optionAttribute.Name.IsNullOrWhiteSpace())
                    {
                        alias.Add("--" + optionAttribute.Name);
                    }

                    AddOption(new Option(alias.ToArray(), optionAttribute.Description)
                    {
                        Argument = new Argument
                        {
                            ArgumentType = propertyInfo.PropertyType,
                        },
                        IsRequired = optionAttribute.Required
                    });

                    continue;
                }

                var argumentAttribute = propertyInfo.GetCustomAttribute<ArgumentAttribute>();
                if (argumentAttribute != null)
                {
                    AddArgument(new Argument(argumentAttribute.Name)
                    {
                        ArgumentType = propertyInfo.PropertyType,
                        Description = argumentAttribute.Description
                    });
                }
            }
        }
    }
}