using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Service
{
    public class ServiceCommandOption : GenerateCommandOption
    {
        [Argument("name", Description = "The service name(without 'AppService' postfix)")]
        public string Name { get; set; } = null!;

        [Option('f', "folder", Description = "Specify the folder where the service files are generated. If not specified, the plural form of the service name is used. Multi-level(e.g., foo/bar) directory is supported")]
        public string Folder { get; set; } = string.Empty;
    }
}