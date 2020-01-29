using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Volo.Abp;

namespace AbpHelper
{
    class Program
    {
        static void Main(string[] args)
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

                Console.WriteLine("Press ENTER to stop application...");
                Console.ReadLine();
            }
        }
    }
}