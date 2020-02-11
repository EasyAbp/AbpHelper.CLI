using System;
using System.IO;
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

                if (args.Length != 2)
                {
                    Console.WriteLine(@"Usage: abphelper abp_solution_dir entity_file_name");
                    Console.WriteLine(@"Example: abphelper c:\MyAbpApp book.cs");
                    return;
                }

                var dir = args[0];
                var entityFile = args[1];
                await Execute(application.ServiceProvider, dir, entityFile);
            }
        }

        private static async Task Execute(IServiceProvider serviceProvider, string dir, string entityFile)
        {
            var workflowBuilderFactory = serviceProvider.GetRequiredService<Func<IWorkflowBuilder>>();
            var workflowBuilder = workflowBuilderFactory();
            var workflowDefinition = workflowBuilder
                .StartWith<SetVariable>(
                    step =>
                    {
                        step.VariableName = "BaseDirectory";
                        step.ValueExpression = new LiteralExpression(dir);
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "Overwrite";
                        step.ValueExpression = new JavaScriptExpression<bool>("true");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new LiteralExpression(entityFile); })
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