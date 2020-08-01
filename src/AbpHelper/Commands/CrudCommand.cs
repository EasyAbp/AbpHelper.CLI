using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow.Generate.Crud;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using System;
using System.Linq;

namespace EasyAbp.AbpHelper.Commands
{
    public class CrudCommand : CommandWithOption<CrudCommandOptions>
    {
        private const string DbMigrations = "DbMigrations";
        private const string TestGeneration = "TestGeneration";

        public CrudCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "crud", "Generate a set of CRUD related files according to the specified entity")
        {
        }

        protected override IActivityBuilder ConfigureBuild(CrudCommandOptions option, IActivityBuilder activityBuilder)
        {
            var entityFileName = option.Entity + ".cs";

            return base.ConfigureBuild(option, activityBuilder)
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
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{nameof(CrudCommandOptions.SkipEntityConstructors)}"),
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
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{nameof(CrudCommandOptions.CustomRepository)}"),
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
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{nameof(CrudCommandOptions.SkipLocalization)}"),
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
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{nameof(CrudCommandOptions.SkipUi)}"),
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
                                        .Then(TestGeneration);

                                    @switch.When(UiFramework.RazorPages.ToString("D"))
                                        .AddUiRazorPagesGenerationWorkflow()
                                        .Then(TestGeneration);

                                    @switch.When(UiFramework.Angular.ToString("D"))
                                        // TODO
                                        //.AddUiAngularGenerationWorkflow()
                                        .Then(TestGeneration);
                                }
                            )
                            ;
                        ifElse
                            .When(OutcomeNames.True)
                            .Then(TestGeneration)
                            ;
                    }
                ).WithName("Ui")
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{nameof(CrudCommandOptions.SkipTest)}"),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .AddTestGenerationWorkflow()
                            .Then(DbMigrations)
                            ;
                        ifElse
                            .When(OutcomeNames.True)
                            .Then(DbMigrations)
                            ;
                    }
                ).WithName(TestGeneration)
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{nameof(CrudCommandOptions.SkipDbMigrations)}"),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .AddMigrationAndUpdateDatabaseWorkflow()
                            ;
                    }
                ).WithName(DbMigrations);
        }
    }
}