using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow.Common;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class MigrationAndUpdateDatabaseWorkflow
    {
        public static IActivityBuilder AddMigrationAndUpdateDatabaseWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    .Then<EmptyStep>()
                    .AddConfigureMigrationProjectsWorkflow(ActivityNames.AddMigration)
                    /* Add migration */
                    .Then<RunCommandStep>(step =>
                    {
                        step.Set(x => x.Command, x =>
                        {
                            var entityInfo = x.GetVariable<EntityInfo>("EntityInfo")!;
                            var migrationProjectFile = x.GetVariable<string>("MigrationProjectFile");
                            var startupProjectFile = x.GetVariable<string>("StartupProjectFile");
                            return
                                $"dotnet ef migrations add Added{entityInfo.Name} -p \"{migrationProjectFile}\" -s \"{startupProjectFile}\"";
                        });
                    }).WithName(ActivityNames.AddMigration)
                    /* Update database */
                    .Then<RunCommandStep>(step =>
                    {
                        step.Set(x => x.Command, x =>
                        {
                            var migrationProjectFile = x.GetVariable<string>("MigrationProjectFile");
                            var startupProjectFile = x.GetVariable<string>("StartupProjectFile");
                            return
                                $"dotnet ef database update -p \"{migrationProjectFile}\" -s \"{startupProjectFile}\"";
                        });
                    })
                ;
        }
    }
}