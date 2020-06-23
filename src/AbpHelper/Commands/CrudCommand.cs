using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Extensions;
using DosSEdo.AbpHelper.Models;
using DosSEdo.AbpHelper.Steps.Abp;
using DosSEdo.AbpHelper.Steps.Common;
using DosSEdo.AbpHelper.Workflow.Generate.Crud;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;

namespace DosSEdo.AbpHelper.Commands
{
    public class CrudCommand : CommandBase
    {
        public CrudCommand(IServiceProvider serviceProvider) : base(serviceProvider, "crud", "Generate a set of CRUD related files according to the specified entity")
        {
            AddArgument(new Argument<string>("entity"){Description = "The entity class name"});
            AddOption(new Option(new[] {"-d", "--directory"}, "The ABP project root directory. If no directory is specified, current directory is used")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"--separate-dto"}, "Generate separate Create and Update DTO files")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--skip-permissions"}, "Skip generating crud permissions")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--custom-repository"}, "Generate custom repository interface and class for the entity")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--skip-db-migrations"}, "Skip performing db migration and update")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--skip-ui"}, "Skip generating UI")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--skip-view-model"}, "Skip generating 'CreateUpdateViewModel`, use 'CreateUpdateDto' directly")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--skip-localization"}, "Skip generating localization")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--skip-test"}, "Skip generating test")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--no-overwrite"}, "Specify not to overwrite existing files")
            {
                Argument = new Argument<bool>()
            });
            AddOption(new Option(new[] {"--migration-project-name"},
                $"Specify the name of the migration project." + Environment.NewLine +
                "For ABP applications, the default value is '*.EntityFrameworkCore.DbMigrations.csproj';" + Environment.NewLine +
                "For ABP modules, the default value is '*.Web.Unified.csproj'." + Environment.NewLine +
                "For example: --migration-project-name *.HttpApi.Host.csproj, abphelper will search '*.HttpApi.Host.csproj' file and make it as the migration project." + Environment.NewLine +
                "This argument takes effect only if '--skip-db-migrations' is NOT specified.")
            {
                Argument = new Argument<string>()
            });
            AddOption(new Option(new[] {"--skip-entity-constructors"}, "Skip generating constructors for the entity")
            {
                Argument = new Argument<bool>()
            });
            Handler = CommandHandler.Create((CommandOption optionType) => Run(optionType));
        }

        private async Task Run(CommandOption option)
        {
            string directory = GetBaseDirectory(option.Directory);
            string entityFileName = option.Entity + ".cs";

            await RunWorkflow(builder => builder
                .StartWith<SetVariable>(
                    step =>
                    {
                        step.VariableName = "BaseDirectory";
                        step.ValueExpression = new LiteralExpression(directory);
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "Option";
                        step.ValueExpression = new JavaScriptExpression<CommandOption>($"({option.ToJson()})");
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "Overwrite";
                        step.ValueExpression = new JavaScriptExpression<bool>("!Option.NoOverwrite");
                    })
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "TemplateDirectory";
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Crud");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new LiteralExpression(entityFileName); })
                .Then<EntityParserStep>()
                .Then<SetModelVariableStep>()
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipEntityConstructors"),
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.False)
                            .AddEntityConstructorsGenerationWorkflow()
                            .Then("EntityUsing")
                            ;
                        ifElse.When(OutcomeNames.True)
                            .Then("EntityUsing")
                            ;
                    })
                .AddEntityUsingGenerationWorkflow("EntityUsing")
                .AddEfCoreConfigurationWorkflow()
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.CustomRepository"),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.True)
                            .AddCustomRepositoryGeneration()
                            .Then("ServiceGeneration")
                            ;
                        ifElse
                            .When(OutcomeNames.False)
                            .Then("ServiceGeneration")
                            ;
                    }
                )
                .AddServiceGenerationWorkflow("ServiceGeneration")
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipLocalization"),
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.False)
                            .AddLocalizationGenerationWorkflow("LocalizationGeneration")
                            .Then("Ui")
                            ;
                        ifElse.When(OutcomeNames.True)
                            .Then("Ui")
                            ;
                    })
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipUi"),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .Then<Switch>(
                                @switch =>
                                {
                                    @switch.Expression = new JavaScriptExpression<string>("(ProjectInfo.UiFramework)");
                                    @switch.Cases = Enum.GetValues(typeof(UiFramework)).Cast<int>().Select(u => u.ToString()).ToArray();
                                },
                                @switch =>
                                {
                                    @switch.When(UiFramework.None.ToString("D"))
                                        .Then("TestGeneration");

                                    @switch.When(UiFramework.RazorPages.ToString("D"))
                                        .AddUiRazorPagesGenerationWorkflow()
                                        .Then("TestGeneration");

                                    @switch.When(UiFramework.Angular.ToString("D"))
                                        // TODO
                                        //.AddUiAngularGenerationWorkflow()
                                        .Then("TestGeneration");
                                }
                            )
                            ;
                        ifElse
                            .When(OutcomeNames.True)
                            .Then("TestGeneration")
                            ;
                    }
                ).WithName("Ui")
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipTest"),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .AddTestGenerationWorkflow()
                            .Then("DbMigrations")
                            ;
                        ifElse
                            .When(OutcomeNames.True)
                            .Then("DbMigrations")
                            ;
                    }
                ).WithName("TestGeneration")
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipDbMigrations"),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .AddMigrationAndUpdateDatabaseWorkflow()
                            ;
                    }
                ).WithName("DbMigrations")
                .Build()
            );
        }

        private class CommandOption
        {
            public string Directory { get; set; } = null!;
            public string Entity { get; set; } = null!;
            public string MigrationProjectName { get; set; } = null!;
            public bool SkipPermissions { get; set; }
            public bool SeparateDto { get; set; }
            public bool CustomRepository { get; set; }
            public bool SkipDbMigrations { get; set; }
            public bool SkipUi { get; set; }
            public bool SkipViewModel { get; set; }
            public bool SkipLocalization { get; set; }
            public bool SkipTest { get; set; }
            public bool NoOverwrite { get; set; }
            public bool SkipEntityConstructors { get; set; }
        }
    }
}