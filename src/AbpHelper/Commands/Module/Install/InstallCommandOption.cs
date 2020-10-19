using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands.Module.Install
{
    public class InstallCommandOption : ModuleCommandOption
    {
        [Option('v', "version", Description = "Specify the version of the package(s) to install")]
        public string Version { get; set; } = null!;
    }
}