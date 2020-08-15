using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow.Common;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Generate.Crud
{
    public static class MigrationAndUpdateDatabaseWorkflow
    {
        public static IActivityBuilder AddMigrationAndUpdateDatabaseWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    .Then<ToActivityBuilder>()
                    .AddConfigureMigrationProjectsWorkflow()
                    /* Add migration */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef migrations add Added${EntityInfo.Name} -p \"${MigrationProjectFile}\" -s \"${StartupProjectFile}\"`")
                    ).WithName(ActivityNames.RunMigration)
                    /* Update database */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef database update -p \"${MigrationProjectFile}\" -s \"${StartupProjectFile}\"`")
                    )
                ;
        }
    }
}