using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Extensions;
using DosSEdo.AbpHelper.Steps.Abp;
using DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;

namespace DosSEdo.AbpHelper.Commands
{
    public class MethodsCommand : CommandBase
    {
        public MethodsCommand(IServiceProvider serviceProvider) : base(serviceProvider, "methods", "Generate service method(s) according to the specified name(s)")
        {
            AddArgument(new Argument<string[]>("method-names") {Description = "The method names"});
            AddOption(new Option(new[] {"-s", "--service-name"}, "The service name(without 'AppService' postfix)")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"-d", "--directory"}, "The ABP project root directory. If no directory is specified, current directory is used")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"--no-overwrite"}, "Specify not to overwrite existing files")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--no-input"}, "Not to generate input DTO file and parameter")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--no-output"}, "Not to generate output DTO file and parameter")
            {
                Argument = new Argument<bool>()
            });
            Handler = CommandHandler.Create((CommandOption optionType) => Run(optionType));
        }

        private async Task Run(CommandOption option)
        {
            string directory = GetBaseDirectory(option.Directory);
            for (int i = 0; i < option.MethodNames.Length; i++)
            {
                // Convert method name to pascal case
                option.MethodNames[i] = option.MethodNames[i].ToPascalCase();
            }

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
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Methods");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new JavaScriptExpression<string>("`I${Option.ServiceName}AppService.cs`"); })
                .Then<ServiceInterfaceParserStep>()
                .Then<SetModelVariableStep>()
                .Then<AppServiceInterfaceStep>()
                .Then<FileModifierStep>()
                .Then<ForEach>(
                    x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>("Option.MethodNames"); },
                    branch =>
                        branch.When(OutcomeNames.Iterate)
                            .Then<SetVariable>(
                                step =>
                                {
                                    step.VariableName = "Bag.Name";
                                    step.ValueExpression = new JavaScriptExpression<string>("CurrentValue");
                                }
                            )
                            .Then<SetModelVariableStep>()
                            .Then<GroupGenerationStep>(
                                step =>
                                {
                                    step.GroupName = "Service";
                                    step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                                }
                            )
                            .Then(branch)
                )
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new JavaScriptExpression<string>("`${Option.ServiceName}AppService.cs`"); })
                .Then<AppServiceClassStep>()
                .Then<FileModifierStep>()
                .Build()
            );
        }

        private class CommandOption
        {
            public string Directory { get; set; } = null!;
            public bool Overwrite { get; set; }
            public string ServiceName { get; set; } = null!;
            public string[] MethodNames { get; set; } = null!;
            public bool NoInput { get; set; }
            public bool NoOutput { get; set; }
        }
    }
}