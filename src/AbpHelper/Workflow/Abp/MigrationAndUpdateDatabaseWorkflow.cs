using AbpHelper.Extensions;
using AbpHelper.Models;
using AbpHelper.Steps;

namespace AbpHelper.Workflow.Abp
{
    public static class MigrationAndUpdateDatabaseWorkflow
    {
        public static IActivityBuilder AddMigrationAndUpdateDatabaseWorkflow(this IActivityBuilder builder)
        {
            return builder
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = "*.EntityFrameworkCore.DbMigrations.csproj",
                        step => step.ResultParameterName = "MigrationProjectFile"
                    )
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = "*.Web.csproj",
                        step => step.ResultParameterName = "WebProjectFile"
                    )
                    /* Add migration */
                    .Then<RunCommandStep>(
                        step =>
                        {
                            var entityInfo = step.Get<EntityInfo>();
                            var migrationProjectFile = step.GetParameter<string>("MigrationProjectFile");
                            var webProjectFile = step.GetParameter<string>("WebProjectFile");
                            step.Command = $"dotnet ef migrations add Added{entityInfo.Name} -p \"{migrationProjectFile}\" -s \"{webProjectFile}\"";
                        })
                    /* Update database */
                    .Then<RunCommandStep>(
                        step =>
                        {
                            var migrationProjectFile = step.GetParameter<string>("MigrationProjectFile");
                            var webProjectFile = step.GetParameter<string>("WebProjectFile");
                            step.Command = $"dotnet ef database update -p \"{migrationProjectFile}\" -s \"{webProjectFile}\"";
                        })
                ;
        }
    }
}