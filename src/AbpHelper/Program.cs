using System;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Steps;
using AbpHelper.Workflow;
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

                Console.WriteLine("Press ENTER to stop application...");
                Console.ReadLine();
            }
        }

        private static async Task Execute(IServiceProvider serviceProvider)
        {
            var workflow = WorkflowBuilder.CreateBuilder(serviceProvider)
                .AddStep<ProjectInfoProviderStep>(step => step.ProjectBaseDirectory = @"C:\Users\wakuw\Desktop\AbpApp\MyAbpRazorPages")
                .AddStep<FileFinderStep>(step => step.SearchFileName = "*DbContextModelCreatingExtensions.cs")
                /*
                     .AddStep<TextGenerationStep>()
                     .WithParameter("TemplateFile", "dummyTemplateFile")
                     .WithParameter("Model", new object())
                     .AddStep<FileModifierStep>().WithParameter("Modifications", new List<Modification>
                     {
                         new Deletion(16, 21),
                         new Insertion(22, "CODE\r\n"),
                         new Insertion(30, "// End of File", InsertPosition.After)
                     })
                     */
                .Build();

            await workflow.Run();
        }
    }
}