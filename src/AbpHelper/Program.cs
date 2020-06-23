using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using CommandLineBuilder = EasyAbp.AbpHelper.Commands.CommandLineBuilder;

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
                .WriteTo.Console()
                .CreateLogger();


            using (var application = AbpApplicationFactory.Create<AbpHelperModule>(options =>
            {
                options.UseAutofac();
                options.Services.AddLogging(c => c.AddSerilog());
            }))
            {
                application.Initialize();

                var parser = new CommandLineBuilder(application.ServiceProvider, "abphelper")
                    .AddCommand<GenerateCommand>()
                    .UseDefaults()
                    .Build();

                await parser.InvokeAsync(args);
            }
        }
    }
}