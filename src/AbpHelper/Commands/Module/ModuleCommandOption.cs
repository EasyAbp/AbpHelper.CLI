using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands.Module
{
    public abstract class ModuleCommandOption : CommandOptionsBase
    {
        [Argument("module-name", Description = "The module name")]
        public string ModuleName { get; set; } = null!;

        [Option('s', "shared", Description = "Install the {module-name}.Domain.Shared nuget package")]
        public bool Shared { get; set; }

        [Option('o', "domain", Description = "Install the {module-name}.Domain nuget package")]
        public bool Domain { get; set; }

        [Option('e', "entity-framework-core", Description = "Install the {module-name}.EntityFrameworkCore nuget package")]
        public bool EntityFrameworkCore { get; set; }

        [Option('m', "mongo-db", Description = "Install the {module-name}.MongoDB nuget package")]
        public bool MongoDB { get; set; }

        [Option('c', "contract", Description = "Install the {module-name}.Contract nuget package")]
        public bool Contract { get; set; }

        [Option('a', "application", Description = "Install the {module-name}.Application nuget package")]
        public bool Application { get; set; }

        [Option('h', "http-api", Description = "Install the {module-name}.HttpApi nuget package")]
        public bool HttpApi { get; set; }

        [Option('l', "client", Description = "Install the {module-name}.HttpApi.Client nuget package")]
        public bool Client { get; set; }

        [Option('w', "web", Description = "Install the {module-name}.Web nuget package")]
        public bool Web { get; set; }
    }
}