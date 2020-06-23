using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using CommandLineBuilder = DosSEdo.AbpHelper.Commands.CommandLineBuilder;

namespace DosSEdo.AbpHelper
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


            using (IAbpApplicationWithInternalServiceProvider application = AbpApplicationFactory.Create<AbpHelperModule>(options =>
            {
                options.UseAutofac();
                options.Services.AddLogging(c => c.AddSerilog());
            }))
            {
                application.Initialize();

                Parser parser = new CommandLineBuilder(application.ServiceProvider, "abphelper")
                    .AddCommand<GenerateCommand>()
                    .UseDefaults()
                    .Build();

                await parser.InvokeAsync(args);
            }
        }
    }
}