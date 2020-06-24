using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;

namespace EasyAbp.AbpHelper.Commands
{
    public class ControllerCommand : CommandBase
    {
        public ControllerCommand(IServiceProvider serviceProvider) : base(serviceProvider, "controller", "Generate controller class and methods according to the specified service")
        {
            AddArgument(new Argument<string>("name") {Description = "The service name(without 'AppService' postfix)"});
            AddOption(new Option(new[] {"-d", "--directory"}, "The ABP project root directory. If no directory is specified, current directory is used")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"-f", "--folder"}, "Specify the folder where the controller is generated. Multi-level(e.g., foo/bar) directory is supported")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"-s", "--startup-project-name"}, "Specify the name of the startup project. For ABP applications, the default value is '*.Web.csproj'; For ABP modules, the default value is '*.HttpApi.Host.csproj'")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"--skip-build"}, "Skip building the solution")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--regenerate"}, "Completely regenerate the controller class, instead of the default: only generate the missing controller methods")
            {
                Argument = new Argument<bool>()
            });
            Handler = CommandHandler.Create((CommandOption optionType) => Run(optionType));
        }

        private async Task Run(CommandOption option)
        {
            string directory = GetBaseDirectory(option.Directory);
            option.Folder = option.Folder.NormalizePath();
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
                        step.VariableName = "TemplateDirectory";
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Controller");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<RunCommandStep>(
                    step => step.Command = new JavaScriptExpression<string>(
                        @"`cd /d ${AspNetCoreDir} && dotnet build`"
                    ))
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new JavaScriptExpression<string>("`I${Option.Name}AppService.cs`"); })
                .Then<ServiceInterfaceParserStep>()
                .Then<SetModelVariableStep>()
                .Build()
            );
        }

        private class CommandOption
        {
            public string Directory { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string Folder { get; set; } = String.Empty;
            public bool SkipBuild { get; set; }
            public bool Regenerate { get; set; }
        }
    }
}