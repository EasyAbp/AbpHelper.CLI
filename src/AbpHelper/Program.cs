using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Dtos;
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
            var parameters = new Dictionary<string, object>
            {
                {"BaseDirectory", @"C:\Users\wakuw\Desktop"},
                {"SearchFileName", "BookStoreDbContextModelCreatingExtensions.cs"},
                {"TemplateFile", "dummyTemplateFile"},
                {"Model", new object()},
                {
                    "Modifications", new List<Modification>
                    {
                        new Deletion(16, 21),
                        new Insertion(22, "CODE\r\n"),
                        new Insertion(30, "// End of File", InsertPosition.After)
                    }
                }
            };
            /*
            var steps = new IStep[]
            {
                new FileFinderStep(),
                new TextGenerationStep(),
                new FileModifierStep()
            };

            foreach (var step in steps)
            {
                await step.Run(parameters);
            }
            */
        }
    }
}