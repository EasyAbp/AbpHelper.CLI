using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Extensions;
using AbpHelper.Models;
using AbpHelper.Steps;
using AbpHelper.Steps.CSharp;
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

                Console.WriteLine("Done.");
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
                .AddStep<EntityParserStep>()
                .AddStep<TextGenerationStep>(
                    step => step.TemplateFile = "EntityConfiguration",
                    step => step.Model = new
                    {
                        EntityInfo = step.GetParameter<EntityInfo>("EntityInfo"),
                        ProjectInfo = step.GetParameter<ProjectInfo>("ProjectInfo")
                    })
                .AddStep<FileFinderStep>(
                    step => step.BaseDirectory = step.GetParameter<string>("ProjectBaseDirectory"),
                    step => step.SearchFileName = "*DbContextModelCreatingExtensions.cs"
                )
                .AddStep<ModificationCreatorStep>(
                    step =>
                    {
                        var usingEntity = $"using {step.GetParameter<EntityInfo>("EntityInfo").Namespace};{Environment.NewLine}";
                        var usingModeling = $"using Volo.Abp.EntityFrameworkCore.Modeling;{Environment.NewLine}";
                        step.ModificationBuilders = new List<ModificationBuilder>
                        {
                            new InsertionBuilder(root => 1, usingEntity, shouldModifier: root => root.NotExist<UsingDirectiveSyntax>(usingEntity)),
                            new InsertionBuilder(root => 4, usingModeling, shouldModifier: root => root.NotExist<UsingDirectiveSyntax>(usingModeling)),
                            new InsertionBuilder(root => root.Descendants<MethodDeclarationSyntax>().First().GetEndLine(), step.GetParameter<string>("GeneratedText"))
                        };
                    })
                .AddStep<FileModifierStep>(
                    step => step.Modifications = step.GetParameter<IList<Modification>>("Modifications")
                )
                .Build();

            await workflow.Run();
        }
    }
}