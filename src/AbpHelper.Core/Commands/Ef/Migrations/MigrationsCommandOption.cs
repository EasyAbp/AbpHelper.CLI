using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Ef.Migrations
{
    public abstract class MigrationsCommandOption : CommandOptionsBase
    {
        private const string MigrationProjectNameDescription = @"Specify the name of the migration project.
For ABP applications after v4.4.0, the default value is '*.EntityFrameworkCore.csproj';
For ABP applications before v4.4.0, the default value is '*.EntityFrameworkCore.DbMigrations.csproj';
For ABP modules, the default value is '*.HttpApi.Host.csproj'.
For example: --migration-project-name *.Web.Unified.csproj, abphelper will search '*.Web.Unified.csproj' file and make it as the migration project.
";

        [Option("migration-project-name", Description = MigrationProjectNameDescription)]
        public string MigrationProjectName { get; set; } = null!;
    }
}