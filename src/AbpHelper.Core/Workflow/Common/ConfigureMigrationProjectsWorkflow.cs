using System;
using EasyAbp.AbpHelper.Core.Commands;
using EasyAbp.AbpHelper.Core.Commands.Generate.Crud;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Common
{
    public static class ConfigureMigrationProjectsWorkflow
    {
        private const string StartupProjectFile = nameof(StartupProjectFile);
        private const string MigrationProjectFile = nameof(MigrationProjectFile);
        private const string ModuleMigrationProjectName = nameof(ModuleMigrationProjectName);
        private const string AppMigrationProjectName = nameof(AppMigrationProjectName);

        public static IActivityBuilder AddConfigureMigrationProjectsWorkflow(this IActivityBuilder builder,
            string nextActivityName)
        {
            return builder
                    .AddConfigureHasDbMigrationsWorkflow("SearchMigrationProject")
                    .Then<If>(
                        ifElse =>
                        {
                            ifElse.Set(x => x.Condition, x =>
                            {
                                var option = x.GetVariable<CrudCommandOption>(CommandConsts.OptionVariableName);
                                return option?.MigrationProjectName.IsNullOrWhiteSpace();
                            });
                        },
                        ifElse =>
                        {
                            ifElse
                                .When(OutcomeNames.True) // No migration project name provided
                                .Then<If>(
                                    ifElse1 =>
                                    {
                                        ifElse1.Set(x => x.Condition,
                                            x => x.GetVariable<bool>(VariableNames.HasDbMigrations));
                                    },
                                    ifElse1 =>
                                    {
                                        ifElse1
                                            .When(OutcomeNames.True)
                                            .Then<SetVariable>(step =>
                                            {
                                                step.Set(x => x.VariableName, AppMigrationProjectName);
                                                step.Set(x => x.Value, "*.EntityFrameworkCore.DbMigrations.csproj");
                                            })
                                            .ThenNamed("SearchHttpApiHostProject")
                                            ;
                                        ifElse1
                                            .When(OutcomeNames.False)
                                            .Then<SetVariable>(step =>
                                            {
                                                step.Set(x => x.VariableName, AppMigrationProjectName);
                                                step.Set(x => x.Value, "*.EntityFrameworkCore.csproj");
                                            })
                                            .ThenNamed("SearchHttpApiHostProject")
                                            ;
                                    }
                                )
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, ModuleMigrationProjectName);
                                    step.Set(x => x.Value, "*.HttpApi.Host.csproj");
                                }).WithName("SearchHttpApiHostProject")
                                .ThenNamed(ActivityNames.SearchFiles)
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, AppMigrationProjectName);
                                    step.Set(x => x.Value, x =>
                                    {
                                        var option =
                                            x.GetVariable<CrudCommandOption>(CommandConsts.OptionVariableName)!;
                                        return option.MigrationProjectName;
                                    });
                                })
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, ModuleMigrationProjectName);
                                    step.Set(x => x.Value, x =>
                                    {
                                        var option =
                                            x.GetVariable<CrudCommandOption>(CommandConsts.OptionVariableName)!;
                                        return option.MigrationProjectName;
                                    });
                                })
                                .ThenNamed(ActivityNames.SearchFiles)
                                ;
                        }
                    )
                    .WithName("SearchMigrationProject")
                    .Then<If>(
                        ifElse =>
                        {
                            ifElse.Set(x => x.Condition, x =>
                            {
                                var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                return projectInfo.TemplateType is TemplateType.Application;
                            });
                        },
                        ifElse =>
                        {
                            // Application
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<FileFinderStep>(step =>
                                {
                                    step.Set(x => x.SearchFileName,
                                        x => x.GetVariable<string>(AppMigrationProjectName));
                                    step.Set(x => x.ResultVariableName, MigrationProjectFile);
                                })
                                .Then<If>(
                                    ie =>
                                    {
                                        ie.Set(x => x.Condition, x =>
                                        {
                                            var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                            return projectInfo.UiFramework is UiFramework.RazorPages;
                                        });
                                    },
                                    ie =>
                                    {
                                        ie.When(OutcomeNames.True)
                                            .Then<FileFinderStep>(step =>
                                            {
                                                step.Set(x => x.SearchFileName, "*.Web.csproj");
                                                step.Set(x => x.ResultVariableName, StartupProjectFile);
                                            })
                                            .ThenNamed(nextActivityName)
                                            ;
                                    })
                                .Then<If>(
                                    ie =>
                                    {
                                        ie.Set(x => x.Condition, x =>
                                        {
                                            var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                            return projectInfo.UiFramework is UiFramework.None;
                                        });
                                    },
                                    ie =>
                                    {
                                        ie.When(OutcomeNames.True)
                                            .Then<FileFinderStep>(step =>
                                            {
                                                step.Set(x => x.SearchFileName, "*.DbMigrator.csproj");
                                                step.Set(x => x.ResultVariableName, StartupProjectFile);
                                            })
                                            .ThenNamed(nextActivityName)
                                            ;
                                    });
                            // Module
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(step =>
                                {
                                    step.Set(x => x.SearchFileName,
                                        x => x.GetVariable<string>(ModuleMigrationProjectName));
                                    step.Set(x => x.ResultVariableName, MigrationProjectFile);
                                })
                                .Then<FileFinderStep>(step =>
                                {
                                    step.Set(x => x.SearchFileName,
                                        x => x.GetVariable<string>(
                                            ModuleMigrationProjectName)); // For module, the startup project is same with the migration project
                                })
                                .ThenNamed(nextActivityName)
                                ;
                        }
                    ).WithName(ActivityNames.SearchFiles)
                ;
        }
    }
}