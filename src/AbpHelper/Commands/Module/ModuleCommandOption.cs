using System.Linq;
using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands.Module
{
    public abstract class ModuleCommandOption : CommandOptionsBase
    {
        [Argument("module-name", Description = "The module name")]
        public string ModuleName { get; set; } = null!;

        public string ModuleNameLastPart => ModuleName.Split('.').Last();

        [Option('s', ModuleConsts.Shared, Description = "Install the {module-name}.Domain.Shared nuget package")]
        public bool Shared { get; set; }

        [Option('o', ModuleConsts.Domain, Description = "Install the {module-name}.Domain nuget package")]
        public bool Domain { get; set; }

        [Option('e', ModuleConsts.EntityFrameworkCore, Description = "Install the {module-name}.EntityFrameworkCore nuget package")]
        public bool EntityFrameworkCore { get; set; }

        [Option('m', ModuleConsts.MongoDB, Description = "Install the {module-name}.MongoDB nuget package")]
        public bool MongoDb { get; set; }

        [Option('c', ModuleConsts.Contracts, Description = "Install the {module-name}.Contract nuget package")]
        public bool Contract { get; set; }

        [Option('a', ModuleConsts.Application, Description = "Install the {module-name}.Application nuget package")]
        public bool Application { get; set; }

        [Option('h', ModuleConsts.HttpApi, Description = "Install the {module-name}.HttpApi nuget package")]
        public bool HttpApi { get; set; }

        [Option('l', ModuleConsts.Client, Description = "Install the {module-name}.HttpApi.Client nuget package")]
        public bool Client { get; set; }

        [Option('w', ModuleConsts.Web, Description = "Install the {module-name}.Web nuget package")]
        public bool Web { get; set; }
    }
}