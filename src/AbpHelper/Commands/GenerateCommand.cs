using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow.Abp;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Commands
{
    public class GenerateCommand : CommandBase
    {
        public GenerateCommand() : base("generate", "Generate a set of CRUD related files according to the specified entity")
        {
            AddOption(new Option(new[] {"-e", "--entity"}, "The entity class name")
            {
                Argument = new Argument<string>(),
                Required = true
            });
            AddOption(new Option(new[] {"-d", "--directory"}, "The ABP project root directory. If no directory is specified, current directory is used.")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"--separate-dto"}, "Generate separate Create and Update DTO file")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--custom-repository"}, "Generate custom repository interface and class for the entity")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--skip-db-migrations"}, "Skip performing db migration and update")
            {
                Argument = new Argument<bool>()
            });
            Handler = CommandHandler.Create((CommandOption optionType) => Run(optionType));
        }

        private async Task Run(CommandOption option)
        {
            var directory = option.Directory;
            if (directory.IsNullOrEmpty())
            {
                directory = Environment.CurrentDirectory;
            }
            else if (!Directory.Exists(directory))
            {
                Logger.LogError($"Directory '{directory}' does not exist.");
                return;
            }

            Logger.LogInformation($"Use directory: `{directory}`");

            var entityFileName = option.Entity + ".cs";

            var workflowBuilderFactory = ServiceProvider.GetRequiredService<Func<IWorkflowBuilder>>();
            var workflowBuilder = workflowBuilderFactory();
            var workflowDefinition = workflowBuilder
                .StartWith<SetVariable>(
                    step =>
                    {
                        step.VariableName = "BaseDirectory";
                        step.ValueExpression = new LiteralExpression(directory);
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
                        step.VariableName = "Option";
                        step.ValueExpression = new JavaScriptExpression<CommandOption>($"({option.ToJson()})");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new LiteralExpression(entityFileName); })
                .Then<EntityParserStep>()
                .Then<SetModelVariableStep>()
                .AddEntityUsingGenerationWorkflow()
                .AddEfCoreConfigurationWorkflow()
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.CustomRepository"),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.True)
                            .AddCustomRepositoryGeneration()
                            .Then("ServiceGeneration")
                            ;
                        ifElse
                            .When(OutcomeNames.False)
                            .Then("ServiceGeneration")
                            ;
                    }
                )
                .AddServiceGenerationWorkflow("ServiceGeneration")
                .Then<Switch>(
                    @switch =>
                    {
                        @switch.Expression = new JavaScriptExpression<string>("(ProjectInfo.UiFramework)");
                        @switch.Cases = Enum.GetValues(typeof(UiFramework)).Cast<int>().Select(u => u.ToString()).ToArray();
                    },
                    @switch =>
                    {
                        @switch.When(UiFramework.None.ToString("D"))
                            .Then("TestGeneration");

                        @switch.When(UiFramework.RazorPages.ToString("D"))
                            .AddUiRazorPagesGenerationWorkflow()
                            .Then("TestGeneration");

                        @switch.When(UiFramework.Angular.ToString("D"))
                            // TODO
                            //.AddUiAngularGenerationWorkflow()
                            .Then("TestGeneration");
                    }
                )
                .AddTestGenerationWorkflow("TestGeneration")
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipDbMigrations"),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .AddMigrationAndUpdateDatabaseWorkflow()
                            ;
                    }
                )
                .Build();

            // Start the workflow.
            var invoker = ServiceProvider.GetService<IWorkflowInvoker>();
            await invoker.StartAsync(workflowDefinition);
        }

        private class CommandOption
        {
            public string Directory { get; set; } = null!;
            public string Entity { get; set; } = null!;
            public bool SeparateDto { get; set; }
            public bool CustomRepository { get; set; }
            public bool SkipDbMigrations { get; set; }
        }
    }
}