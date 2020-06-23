using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Extensions;
using DosSEdo.AbpHelper.Steps.Abp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;

namespace DosSEdo.AbpHelper.Commands
{
    public class LocalizationCommand : CommandBase
    {
        public LocalizationCommand(IServiceProvider serviceProvider) : base(serviceProvider, "localization", "Generate localization item(s) according to the specified name(s)")
        {
            AddArgument(new Argument<string[]>("names") {Description = "The localization item names, separated by the space char"});
            AddOption(new Option(new[] {"-d", "--directory"}, "The ABP project root directory. If no directory is specified, current directory is used")
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
                        step.VariableName = "Option";
                        step.ValueExpression = new JavaScriptExpression<CommandOption>($"({option.ToJson()})");
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "TemplateDirectory";
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Localization");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<SetModelVariableStep>()
                /* Add localization */
                .Then<TextGenerationStep>(
                    step => { step.TemplateName = "Localization"; }
                )
                .Then<MultiFileFinderStep>(
                    step =>
                    {
                        step.SearchFileName = new LiteralExpression("*.json");
                        step.BaseDirectory = new JavaScriptExpression<string>(@"`${AspNetCoreDir}/src/${ProjectInfo.FullName}.Domain.Shared/Localization`");
                    }
                )
                .Then<ForEach>(
                    x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(MultiFileFinderStep.DefaultFileParameterName); },
                    branch =>
                        branch.When(OutcomeNames.Iterate)
                            .Then<LocalizationJsonModificationCreatorStep>(
                                step =>
                                {
                                    step.TargetFile = new JavaScriptExpression<string>("CurrentValue");
                                    step.LocalizationJson = new JavaScriptExpression<string>(TextGenerationStep.DefaultGeneratedTextParameterName);
                                }
                            )
                            .Then(branch)
                )
                .Build()
            );
        }

        private class CommandOption
        {
            public string Directory { get; set; } = null!;
            public string[] Names { get; set; } = null!;
        }
    }
}