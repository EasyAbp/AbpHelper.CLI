using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands
{
    public class ServiceCommandOption : CommandOptionsBase
    {
        [Argument("name", Description = "The service name(without 'AppService' postfix)")]
        public string Name { get; set; } = null!;

        [Option('f', "folder", Description = "Specify the folder where the service files are generated. Multi-level(e.g., foo/bar) directory is supported")]
        public string Folder { get; set; } = string.Empty;
    }
}