using DosSEdo.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace DosSEdo.AbpHelper.Workflow.Generate.Crud
{
    public static class MigrationAndUpdateDatabaseWorkflow
    {
        public static IActivityBuilder AddMigrationAndUpdateDatabaseWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    .Then<IfElse>(
                        ifElse => ifElse.ConditionExpression = new JavaScriptExpression<bool>("Option.MigrationProjectName == null"),
                        ifElse =>
                        {
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = "AppMigrationProjectName";
                                        step.ValueExpression = new LiteralExpression("*.EntityFrameworkCore.DbMigrations.csproj");
                                    })
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = "ModuleMigrationProjectName";
                                        step.ValueExpression = new LiteralExpression("*.Web.Unified.csproj");
                                    })
                                .Then("SearchFiles")
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = "AppMigrationProjectName";
                                        step.ValueExpression = new JavaScriptExpression<string>("Option.MigrationProjectName");
                                    })
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = "ModuleMigrationProjectName";
                                        step.ValueExpression = new JavaScriptExpression<string>("Option.MigrationProjectName");
                                    })
                                .Then("SearchFiles")
                                ;
                        }
                    )
                    .Then<IfElse>(
                        ifElse => ifElse.ConditionExpression = new JavaScriptExpression<bool>("ProjectInfo.TemplateType == 0"),
                        ifElse =>
                        {
                            // Application
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<FileFinderStep>(
                                    step =>
                                    {
                                        step.SearchFileName = new JavaScriptExpression<string>("AppMigrationProjectName");
                                        step.ResultVariableName = new LiteralExpression("MigrationProjectFile");
                                    }
                                )
                                .Then<FileFinderStep>(
                                    step =>
                                    {
                                        step.SearchFileName = new LiteralExpression("*.Web.csproj");
                                        step.ResultVariableName = new LiteralExpression<string>("StartupProjectFile");
                                    }
                                )
                                .Then("RunMigration")
                                ;
                            
                            // Module
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(
                                    step =>
                                    {
                                        step.SearchFileName = new JavaScriptExpression<string>("ModuleMigrationProjectName");
                                        step.ResultVariableName = new LiteralExpression("MigrationProjectFile");
                                    }
                                )
                                .Then<FileFinderStep>(
                                    step =>
                                    {
                                        step.SearchFileName = new JavaScriptExpression<string>("ModuleMigrationProjectName");    // For module, the startup project is same with the migration project
                                        step.ResultVariableName = new LiteralExpression<string>("StartupProjectFile");
                                    }
                                )
                                .Then("RunMigration")
                                ;
                        }
                    ).WithName("SearchFiles")
                    /* Add migration */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef migrations add Added${EntityInfo.Name} -p \"${MigrationProjectFile}\" -s \"${StartupProjectFile}\"`")
                    ).WithName("RunMigration")
                    /* Update database */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef database update -p \"${MigrationProjectFile}\" -s \"${StartupProjectFile}\"`")
                    )
                ;
        }
    }
}