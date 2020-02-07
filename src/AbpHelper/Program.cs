using System;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Steps;
using AbpHelper.Workflow;
using AbpHelper.Workflow.Abp;
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
            var workflow = WorkflowBuilder.CreateBuilder(serviceProvider)
                .WithParameter("BaseDirectory", @"C:\Users\wakuw\Desktop\AbpApp\MyAbpRazorPages")
                .WithParameter("Overwrite", true)
                .AddStep<ProjectInfoProviderStep>()
                .AddStep<FileFinderStep>(
                    step => step.SearchFileName = "Book.cs"
                )
                .AddStep<EntityParserStep>()
                .AddEntityUsingGenerationWorkflow()
                .AddEfCoreConfigurationWorkflow()
                .AddMigrationAndUpdateDatabaseWorkflow()
                .AddServiceGenerationWorkflow()
                .AddUIRazorPagesGenerationWorkflow()
                .Build();

            // TODO: handle exception
            await workflow.Run();
        }
    }
}