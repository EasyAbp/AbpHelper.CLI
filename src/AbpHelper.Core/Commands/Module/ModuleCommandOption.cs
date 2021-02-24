using System.Linq;
using Castle.Core.Internal;
using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Module
{
    public abstract class ModuleCommandOption : CommandOptionsBase
    {
        private string _moduleCompanyName;

        [Argument("module-name", Description = "The module name")]
        public string ModuleName { get; set; } = null!;

        [Option("module-company-name",
            Description =
                "The module company name, it will be set to the part before the first \".\" of the module name if you do not specify.")]
        public string ModuleCompanyName
        {
            get
            {
                if (_moduleCompanyName != null)
                {
                    return _moduleCompanyName;
                }
                
                var parts = ModuleName.Split('.');

                return parts.Length > 1 ? parts[0] : "";

            }
            set => _moduleCompanyName = value;
        }

        public string ModuleGroupNameWithoutCompanyName => ModuleName.Substring(ModuleCompanyName.IsNullOrEmpty() ? 0 : ModuleCompanyName.Length + 1).Replace(".", "");

        [Option('s', ModuleConsts.Shared, Description = "Install the {module-name}.Domain.Shared nuget package")]
        public bool Shared { get; set; }

        [Option('o', ModuleConsts.Domain, Description = "Install the {module-name}.Domain nuget package")]
        public bool Domain { get; set; }

        [Option('e', ModuleConsts.EntityFrameworkCore, Description = "Install the {module-name}.EntityFrameworkCore nuget package")]
        public bool EntityFrameworkCore { get; set; }

        [Option('m', ModuleConsts.MongoDB, Description = "Install the {module-name}.MongoDB nuget package")]
        public bool MongoDb { get; set; }

        [Option('c', ModuleConsts.Contracts, Description = "Install the {module-name}.Application.Contracts nuget package")]
        public bool Contracts { get; set; }

        [Option('a', ModuleConsts.Application, Description = "Install the {module-name}.Application nuget package")]
        public bool Application { get; set; }

        [Option('h', ModuleConsts.HttpApi, Description = "Install the {module-name}.HttpApi nuget package")]
        public bool HttpApi { get; set; }

        [Option('l', ModuleConsts.Client, Description = "Install the {module-name}.HttpApi.Client nuget package")]
        public bool Client { get; set; }

        [Option('w', ModuleConsts.Web, Description = "Install the {module-name}.Web nuget package")]
        public bool Web { get; set; }
        
        [Option("custom", Description = "Specify which module is added to (or removed from) which app project. e.g. \"Web:Web,Orders.Web:Web:Orders\" means the Web module and the Orders.Web module were added to (or removed from) the Web app project, the last optional \"Orders\" is the submodule name used to make up the using namespace.")]
        public string Custom { get; set; }
    }
}