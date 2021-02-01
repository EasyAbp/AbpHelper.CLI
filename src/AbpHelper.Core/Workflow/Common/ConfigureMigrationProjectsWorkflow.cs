using EasyAbp.AbpHelper.Core.Commands;
using EasyAbp.AbpHelper.Core.Commands.Generate.Crud;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Workflow.Common
{
    public static class ConfigureMigrationProjectsWorkflow
    {
        private const string StartupProjectFile = nameof(StartupProjectFile);
        private const string MigrationProjectFile = nameof(MigrationProjectFile);
        private const string ModuleMigrationProjectName = nameof(ModuleMigrationProjectName);
        private const string AppMigrationProjectName = nameof(AppMigrationProjectName);

        public static IActivityBuilder AddConfigureMigrationProjectsWorkflow(this IActivityBuilder builder, string nextActivityName)
        {
            return builder
                    .Then<IfElse>(
                        ifElse => ifElse.ConditionExpression = new JavaScriptExpression<bool>($"{CommandConsts.OptionVariableName}.{nameof(CrudCommandOption.MigrationProjectName)} == null"),
                        ifElse =>
                        {
                            ifElse
                                .When(OutcomeNames.True)    // No migration project name provided
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = AppMigrationProjectName;
                                        step.ValueExpression = new LiteralExpression("*.EntityFrameworkCore.DbMigrations.csproj");
                                    })
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = ModuleMigrationProjectName;
                                        step.ValueExpression = new LiteralExpression("*.HttpApi.Host.csproj");
                                    })
                                .Then(ActivityNames.SearchFiles)
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = AppMigrationProjectName;
                                        step.ValueExpression = new JavaScriptExpression<string>($"{CommandConsts.OptionVariableName}.{nameof(CrudCommandOption.MigrationProjectName)}");
                                    })
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = ModuleMigrationProjectName;
                                        step.ValueExpression = new JavaScriptExpression<string>($"{CommandConsts.OptionVariableName}.{nameof(CrudCommandOption.MigrationProjectName)}");
                                    })
                                .Then(ActivityNames.SearchFiles)
                                ;
                        }
                    )
                    .Then<IfElse>(
                        ifElse => ifElse.ConditionExpression = new JavaScriptExpression<bool>
                            ($"ProjectInfo.TemplateType == {TemplateType.Application:D}"),
                        ifElse =>
                        {
                            // Application
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<FileFinderStep>(
                                    step =>
                                    {
                                        step.SearchFileName = new JavaScriptExpression<string>(AppMigrationProjectName);
                                        step.ResultVariableName = new LiteralExpression(MigrationProjectFile);
                                    }
                                )
                                .Then<IfElse>(
                                    ie => ie.ConditionExpression = new JavaScriptExpression<bool>
                                        ($"ProjectInfo.UiFramework == {UiFramework.RazorPages:D}"),
                                    ie =>
                                    {
                                        ie.When(OutcomeNames.True)
                                            .Then<FileFinderStep>(
                                                step =>
                                                {
                                                    step.SearchFileName = new LiteralExpression("*.Web.csproj");
                                                    step.ResultVariableName = new LiteralExpression<string>(StartupProjectFile);
                                                })
                                            .Then(nextActivityName)
                                            ;
                                    })
                                .Then<IfElse>(
                                    ie => ie.ConditionExpression = new JavaScriptExpression<bool>
                                        ($"ProjectInfo.UiFramework == {UiFramework.None:D}"),
                                    ie =>
                                    {
                                        ie.When(OutcomeNames.True)
                                            .Then<FileFinderStep>(
                                                step =>
                                                {
                                                    step.SearchFileName = new LiteralExpression("*.DbMigrator.csproj");
                                                    step.ResultVariableName = new LiteralExpression<string>(StartupProjectFile);
                                                })
                                            .Then(nextActivityName)
                                            ;
                                    });
                            // Module
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(
                                    step =>
                                    {
                                        step.SearchFileName = new JavaScriptExpression<string>(ModuleMigrationProjectName);
                                        step.ResultVariableName = new LiteralExpression(MigrationProjectFile);
                                    }
                                )
                                .Then<FileFinderStep>(
                                    step =>
                                    {
                                        step.SearchFileName = new JavaScriptExpression<string>(ModuleMigrationProjectName); // For module, the startup project is same with the migration project
                                        step.ResultVariableName = new LiteralExpression<string>(StartupProjectFile);
                                    }
                                )
                                .Then(nextActivityName)
                                ;
                        }
                    ).WithName(ActivityNames.SearchFiles)
                ;
        }
    }
}