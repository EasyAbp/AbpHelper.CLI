using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class MigrationAndUpdateDatabaseWorkflow
    {
        public static IActivityBuilder AddMigrationAndUpdateDatabaseWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    .Then<FileFinderStep>(
                        step =>
                        {
                            step.SearchFileName = new LiteralExpression("*.EntityFrameworkCore.DbMigrations.csproj");
                            step.ResultVariableName = new LiteralExpression("MigrationProjectFile");
                        }
                    )
                    .Then<FileFinderStep>(
                        step =>
                        {
                            step.SearchFileName = new LiteralExpression("*.Web.csproj");
                            step.ResultVariableName = new LiteralExpression<string>("WebProjectFile");
                        }
                    )
                    /* Add migration */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef migrations add Added${EntityInfo.Name} -p \"${MigrationProjectFile}\" -s \"${WebProjectFile}\"`")
                    )
                    /* Update database */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef database update -p \"${MigrationProjectFile}\" -s \"${WebProjectFile}\"`")
                    )
                ;
        }
    }
}