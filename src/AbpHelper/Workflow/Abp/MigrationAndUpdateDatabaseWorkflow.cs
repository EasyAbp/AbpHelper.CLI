using AbpHelper.Extensions;
using AbpHelper.Models;
using AbpHelper.Steps;

namespace AbpHelper.Workflow.Abp
{
    public static class MigrationAndUpdateDatabaseWorkflow
    {
        public static WorkflowBuilder AddMigrationAndUpdateDatabaseWorkflow(this WorkflowBuilder builder)
        {
            return builder
                    .AddStep<FileFinderStep>(
                        step => step.SearchFileName = "*.EntityFrameworkCore.DbMigrations.csproj",
                        step => step.ResultParameterName = "MigrationProjectFile"
                    )
                    .AddStep<FileFinderStep>(
                        step => step.SearchFileName = "*.Web.csproj",
                        step => step.ResultParameterName = "WebProjectFile"
                    )
                    /* Add migration */
                    .AddStep<RunCommandStep>(
                        step =>
                        {
                            var entityInfo = step.Get<EntityInfo>();
                            var migrationProjectFile = step.GetParameter<string>("MigrationProjectFile");
                            var webProjectFile = step.GetParameter<string>("WebProjectFile");
                            step.Command = $"dotnet ef migrations add Added{entityInfo.Name} -p \"{migrationProjectFile}\" -s \"{webProjectFile}\"";
                        })
                    /* Update database */
                    .AddStep<RunCommandStep>(
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