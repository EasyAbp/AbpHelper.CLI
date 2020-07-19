using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

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
            AddOption(new Option(new[] {"--skip-build"}, "Skip building the solution")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--regenerate"}, "Completely regenerate the controller class, instead of the default: only generate the missing controller methods")
            {
                Argument = new Argument<bool>()
            }); 
            AddOption(new Option(new[] { "--ignore-directories" }, "Ignore directories when searching files. Example: -ignore-directories Folder1,Folder2")
            {
                Argument = new Argument<string>()
            });
            Handler = CommandHandler.Create((CommandOption optionType) => Run(optionType));
        }

        private async Task Run(CommandOption option)
        {
            string directory = GetBaseDirectory(option.Directory);
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
                        step.VariableName = "IgnoreDirectories";
                        step.ValueExpression = new LiteralExpression(option.IgnoreDirectories);
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
                        step.ValueExpression = new JavaScriptExpression<bool>("true");
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "TemplateDirectory";
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Controller");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipBuild"),
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.False)
                            .Then<RunCommandStep>(
                                step => step.Command = new JavaScriptExpression<string>(
                                    @"`cd /d ${AspNetCoreDir} && dotnet build`"
                                ))
                            .Then("SearchServiceInterface")
                            ;
                        ifElse.When(OutcomeNames.True)
                            .Then("SearchServiceInterface")
                            ;
                    })
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new JavaScriptExpression<string>("`I${Option.Name}AppService.cs`"); }
                ).WithName("SearchServiceInterface")
                .Then<ServiceInterfaceSemanticParserStep>()
                .Then<SetModelVariableStep>()
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.Regenerate"),
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.True) // Regenerate
                            .Then<GroupGenerationStep>(
                                step =>
                                {
                                    step.GroupName = "Controller";
                                    step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                                })
                            ;
                        ifElse.When(OutcomeNames.False)
                            .Then<FileFinderStep>(
                                step =>
                                {
                                    step.SearchFileName = new JavaScriptExpression<string>("`${Option.Name}Controller.cs`");
                                    step.ErrorIfNotFound = new JavaScriptExpression<bool>("false");
                                }
                            ).WithName("SearchController")
                            .Then<IfElse>(
                                step => step.ConditionExpression = new JavaScriptExpression<bool>("FileFinderResult == null"),
                                found =>
                                {
                                    found.When(OutcomeNames.True)
                                        .Then<GroupGenerationStep>(
                                            step =>
                                            {
                                                step.GroupName = "Controller";
                                                step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                                            })
                                        ;
                                    found.When(OutcomeNames.False)
                                        .Then<ControllerParserStep>()
                                        .Then<ControllerStep>()
                                        .Then<FileModifierStep>()
                                        ;
                                }
                            );
                    })
                .Build()
            );
        }

        private class CommandOption
        {
            public string Directory { get; set; } = null!;
            public string Name { get; set; } = null!;
            public bool SkipBuild { get; set; }
            public bool Regenerate { get; set; }
            public string IgnoreDirectories { get; set; } = null!;
        }
    }
}