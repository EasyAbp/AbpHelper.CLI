using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Steps.Abp;
using Elsa.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;

namespace EasyAbp.AbpHelper.Commands
{
    public class ServiceCommand : CommandBase
    {
        public ServiceCommand(IServiceProvider serviceProvider) : base(serviceProvider, "service", "Generate service interface and class files according to the specified name")
        {
            AddOption(new Option(new[] {"-n", "--name"}, "The service name")
            {
                Argument = new Argument<string>(),
                Required = true
            });
            AddOption(new Option(new[] {"-d", "--directory"}, "The ABP project root directory. If no directory is specified, current directory is used.")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"--no-overwrite"}, "Specify not to overwrite existing files")
            {
                Argument = new Argument<bool>(() => true)
            });
            Handler = CommandHandler.Create((CommandOption optionType) => Run(optionType));
        }

        private async Task Run(CommandOption option)
        {
            string directory = GetBaseDirectory(option.Directory);
            await RunWorkFlow(builder => builder
                .StartWith<SetVariable>(
                    step =>
                    {
                        step.VariableName = "BaseDirectory";
                        step.ValueExpression = new LiteralExpression(directory);
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "Option";
                        step.ValueExpression = new JavaScriptExpression<CommandOption>($"({option.ToJson()})");
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "Overwrite";
                        step.ValueExpression = new JavaScriptExpression<bool>("!Option.NoOverwrite");
                    })
                .Then<ProjectInfoProviderStep>()
                .Build()
            );
        }
        
        private class CommandOption
        {
            public string Directory { get; set; } = null!;
            public string Name { get; set; } = null!;
            public bool Overwrite { get; set; }
        }
    }
}