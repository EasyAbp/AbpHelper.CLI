using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Models;
using AbpHelper.Steps;
using AbpHelper.Workflow;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                .WithParameter("ProjectBaseDirectory", @"C:\Users\wakuw\Desktop\AbpApp\MyAbpRazorPages")
                .AddStep<ProjectInfoProviderStep>(step => step.ProjectBaseDirectory = step.GetParameter<string>("ProjectBaseDirectory"))
                .AddStep<FileFinderStep>(
                    step => step.BaseDirectory = step.GetParameter<string>("ProjectBaseDirectory"),
                    step => step.SearchFileName = "Book.cs"
                )
                .AddStep<EntityParserStep>(
                    step => step.EntitySourceFile = step.GetParameter<string>("FilePathName")
                )
                .AddStep<FileFinderStep>(
                    step => step.BaseDirectory = step.GetParameter<string>("ProjectBaseDirectory"),
                    step => step.SearchFileName = "*DbContextModelCreatingExtensions.cs"
                )
                .AddStep<TextGenerationStep>(
                    step => step.TemplateFile = "dummyTemplateFile",
                    step => step.Model = new object()
                )
                .AddStep<InsertionCreationStep>(
                    step => step.SourceFile = step.GetParameter<string>("FilePathName"),
                    step => step.StartLineFunc = root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().GetLocation().GetLineSpan().EndLinePosition.Line,
                    step => step.Content = step.GetParameter<string>("Text")
                )
                .AddStep<FileModifierStep>(
                    step => step.FilePathName = step.GetParameter<string>("FilePathName"),
                    step => step.Modifications = step.GetParameter<IList<Modification>>("Modifications")
                )
                .Build();

            await workflow.Run();
        }
    }
}