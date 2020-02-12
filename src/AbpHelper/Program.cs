using System;
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
using Serilog;
using Serilog.Events;
using Volo.Abp;

namespace EasyAbp.AbpHelper
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(Path.Combine("Logs", "logs.txt"))
                .WriteTo.Console()
                .CreateLogger();

            using (var application = AbpApplicationFactory.Create<AbpHelperModule>(options =>
            {
                options.UseAutofac();
                options.Services.AddLogging(c => c.AddSerilog());
            }))
            {
                application.Initialize();

                (var entityFileName, var baseDirectory) = ParsingArguments(args);

                await Execute(application.ServiceProvider, entityFileName, baseDirectory);
            }
        }

        private static (string entityFileName, string baseDirectory) ParsingArguments(string[] args)
        {
            string entityFileName;
            string baseDirectory;
            var solutionFile = string.Empty;

            if (args.Length == 1)
            {
                solutionFile = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.sln").FirstOrDefault();
                if (solutionFile == null)
                {
                    Console.WriteLine("No .sln file founded in the current directory.");
                    Environment.Exit(-1);
                }
            }
            else if (args.Length == 2 && args[1].EndsWith("*.sln"))
            {
                solutionFile = args[1];
                if (!File.Exists(solutionFile))
                {
                    Console.WriteLine("The specified solution file does not exist.");
                    Environment.Exit(-1);
                }
            }
            else
            {
                Console.WriteLine(@"Usage: abphelper entity_name [abp_solution_file]");
                Console.WriteLine(@"Example: abphelper book");
                Console.WriteLine(@"Or specific solution file:");
                Console.WriteLine(@"Example: abphelper book c:\Acme.BookStore\Acme.BookStore.sln");
                Environment.Exit(-1);
            }

            entityFileName = args[0] + ".cs";
            baseDirectory = Path.GetDirectoryName(solutionFile)!;

            return (entityFileName, baseDirectory);
        }

        private static async Task Execute(IServiceProvider serviceProvider, string entityFileName, string baseDirectory)
        {
            var workflowBuilderFactory = serviceProvider.GetRequiredService<Func<IWorkflowBuilder>>();
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
                .AddMigrationAndUpdateDatabaseWorkflow()
                .AddServiceGenerationWorkflow()
                .AddUiRazorPagesGenerationWorkflow()
                .AddTestGenerationWorkflow()
                .Build();

            // Start the workflow.
            var invoker = serviceProvider.GetService<IWorkflowInvoker>();
            var context = await invoker.StartAsync(workflowDefinition);
            // foreach (var variable in context.GetVariables()) Console.WriteLine($"[{variable.Key}] : {variable.Value.Value}");
        }
    }
}