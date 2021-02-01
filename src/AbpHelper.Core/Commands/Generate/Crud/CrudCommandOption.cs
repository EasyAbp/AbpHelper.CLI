using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Crud
{
    public class CrudCommandOption : GenerateCommandOption
    {
        private const string MigrationProjectNameDescription = @"Specify the name of the migration project.
For ABP applications, the default value is '*.EntityFrameworkCore.DbMigrations.csproj';
For ABP modules, the default value is '*.HttpApi.Host.csproj'.
For example: --migration-project-name *.Web.Unified.csproj, abphelper will search '*.Web.Unified.csproj' file and make it as the migration project.
This argument takes effect only if '--skip-db-migrations' is NOT specified.";

        [Argument("entity", Description = "The entity class name")]
        public string Entity { get; set; } = null!;

        [Option("migration-project-name", Description = MigrationProjectNameDescription)]
        public string MigrationProjectName { get; set; } = null!;

        [Option("skip-permissions", Description = "Skip generating crud permissions")]
        public bool SkipPermissions { get; set; }

        [Option("separate-dto", Description = "Generate separate Create and Update DTO files")]
        public bool SeparateDto { get; set; }

        [Option("entity-prefix-dto", Description = "Start DTO name with the entity name. If this option is specified, the dto name would be e.g. 'TodoCreateDto', otherwise it would be 'CreateTodoDto'")]
        public bool EntityPrefixDto { get; set; }

        [Option("dto-suffix", Description = "Use a customized DTO name suffix instead of 'Dto'")]
        public string DtoSuffix { get; set; } = null!;

        [Option("skip-custom-repository", Description = "Skip generating custom repository interface and class for the entity")]
        public bool SkipCustomRepository { get; set; }

        [Option("skip-db-migrations", Description = "Skip performing db migration and update")]
        public bool SkipDbMigrations { get; set; }

        [Option("skip-ui", Description = "Skip generating UI")]
        public bool SkipUi { get; set; }

        [Option("skip-view-model", Description = "Skip generating 'CreateUpdateViewModel`, use 'CreateUpdateDto' directly")]
        public bool SkipViewModel { get; set; }

        [Option("skip-localization", Description = "Skip generating localization")]
        public bool SkipLocalization { get; set; }

        [Option("skip-test", Description = "Skip generating test")]
        public bool SkipTest { get; set; }

        [Option("skip-entity-constructors", Description = "Skip generating constructors for the entity")]
        public bool SkipEntityConstructors { get; set; }
    }
}