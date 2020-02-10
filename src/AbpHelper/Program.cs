using System;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Steps.Abp;
using AbpHelper.Steps.Common;
using AbpHelper.Workflow.Abp;
using Elsa.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Volo.Abp;

namespace AbpHelper
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

                await Execute(application.ServiceProvider);

                Console.WriteLine("Done.");
            }
        }

        private static async Task Execute(IServiceProvider serviceProvider)
        {
            /*
            var workflow = WorkflowBuilder.CreateBuilder(serviceProvider)
                .WithParameter("BaseDirectory", @"C:\Users\wakuw\Desktop\AbpApp\MyAbpRazorPages")
                .WithParameter("Overwrite", true)
                .Then<ProjectInfoProviderStep>()
                .Then<FileFinderStep>(
                    step => step.SearchFileName = "Book.cs"
                )
                .Then<EntityParserStep>()
                .AddEntityUsingGenerationWorkflow()
                .AddEfCoreConfigurationWorkflow()
                .AddMigrationAndUpdateDatabaseWorkflow()
                .AddServiceGenerationWorkflow()
                .AddUIRazorPagesGenerationWorkflow()
                .Build();

            // TODO: handle exception
            await workflow.Run();
        */
            var workflowBuilderFactory = serviceProvider.GetRequiredService<Func<IWorkflowBuilder>>();
            var workflowBuilder = workflowBuilderFactory();
            var workflowDefinition = workflowBuilder
                .StartWith<SetVariable>(
                    step =>
                    {
                        step.VariableName = "BaseDirectory";
                        step.ValueExpression = new LiteralExpression(@"C:\Users\wakuw\Desktop\AbpApp\MyAbpRazorPages");
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "Overwrite";
                        step.ValueExpression = new JavaScriptExpression<bool>("true");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new LiteralExpression("Book.cs"); })
                .Then<EntityParserStep>()
                .Then<SetModelVariableStep>()
                .AddEntityUsingGenerationWorkflow()
                .AddEfCoreConfigurationWorkflow()
                // .AddMigrationAndUpdateDatabaseWorkflow()
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