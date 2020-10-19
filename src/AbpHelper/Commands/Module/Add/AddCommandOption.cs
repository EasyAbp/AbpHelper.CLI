using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands.Module.Add
{
    public class AddCommandOption : ModuleCommandOption
    {
        [Option('v', "version", Description = "Specify the version of the package(s) to add")]
        public string Version { get; set; } = null!;
    }
}