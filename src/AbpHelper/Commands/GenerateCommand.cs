using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow.Abp;
using Elsa.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Commands
{
    public class GenerateCommand : CommandBase
    {
        public GenerateCommand() : base("generate", "Generate a set of CRUD related files according to the specified entity.")
        {
            AddOption(new Option(new[] {"-e", "--entity"}, "The entity class name")
            {
                Argument = new Argument<string>(),
                Required = true
            });
            AddOption(new Option(new[] {"-s", "--solution"}, "The ABP solution(.sln) file. If no file is specified, the command searches for a file in the current directory")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"--separate-dto"}, "Generate separate Create and Update DTO files.")
            {
                Argument = new Argument<bool>()
            });
            Handler = CommandHandler.Create((CommandOption optionType) => Run(optionType));
        }

        private async Task Run(CommandOption option)
        {
            var solution = option.Solution;
            if (solution.IsNullOrEmpty())
            {
                var file = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.sln").FirstOrDefault();
                if (file == null)
                {
                    Logger.LogError("No solution file founded.");
                    return;
                }

                Logger.LogInformation($"Solution file founded: `{file}`");
                solution = file;
            }

            var entityFileName = option.Entity + ".cs";
            var baseDirectory = Path.GetDirectoryName(solution)!;

            var workflowBuilderFactory = ServiceProvider.GetRequiredService<Func<IWorkflowBuilder>>();
            var workflowBuilder = workflowBuilderFactory();
            var workflowDefinition = workflowBuilder
                .StartWith<SetVariable>(
                    step =>
                    {
                        step.VariableName = "BaseDirectory";
                        step.ValueExpression = new LiteralExpression(baseDirectory);
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "Overwrite";
                        step.ValueExpression = new JavaScriptExpression<bool>("true");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new LiteralExpression(entityFileName); })
                .Then<EntityParserStep>()
                .Then<SetModelVariableStep>()
                .AddEntityUsingGenerationWorkflow()
                .AddEfCoreConfigurationWorkflow()
                .AddServiceGenerationWorkflow()
                .AddUiRazorPagesGenerationWorkflow()
                .AddTestGenerationWorkflow()
                .AddMigrationAndUpdateDatabaseWorkflow()
                .Build();

            // Start the workflow.
            var invoker = ServiceProvider.GetService<IWorkflowInvoker>();
            var context = await invoker.StartAsync(workflowDefinition);
        }

        private class CommandOption
        {
            public string Solution { get; set; } = null!;
            public string Entity { get; set; } = null!;
            public bool SeparateDto { get; set; }
        }
    }
}