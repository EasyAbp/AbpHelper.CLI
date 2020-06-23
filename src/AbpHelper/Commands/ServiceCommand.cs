using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Extensions;
using DosSEdo.AbpHelper.Steps.Abp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;

namespace DosSEdo.AbpHelper.Commands
{
    public class ServiceCommand : CommandBase
    {
        public ServiceCommand(IServiceProvider serviceProvider) : base(serviceProvider, "service", "Generate service interface and class files according to the specified name")
        {
            AddArgument(new Argument<string>("name") {Description = "The service name(without 'AppService' postfix)"});
            AddOption(new Option(new[] {"-d", "--directory"}, "The ABP project root directory. If no directory is specified, current directory is used")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"--no-overwrite"}, "Specify not to overwrite existing files")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"-f", "--folder"}, "Specify the folder where the service files are generated. Multi-level(e.g., foo/bar) directory is supported")
            {
                Argument = new Argument<string>()
            });
            Handler = CommandHandler.Create((CommandOption optionType) => Run(optionType));
        }

        private async Task Run(CommandOption option)
        {
            string directory = GetBaseDirectory(option.Directory);
            option.Folder = option.Folder.Replace('\\', '/');
            await RunWorkflow(builder => builder
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
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "TemplateDirectory";
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Service");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<SetModelVariableStep>()
                .Then<GroupGenerationStep>(
                    step =>
                    {
                        step.GroupName = "Service";
                        step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                    })
                .Build()
            );
        }

        private class CommandOption
        {
            public string Directory { get; set; } = null!;
            public string Name { get; set; } = null!;
            public bool Overwrite { get; set; }
            public string Folder { get; set; } = null!;
        }
    }
}