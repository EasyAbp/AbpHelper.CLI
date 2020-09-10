using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands.Generate
{
    public class GenerateCommandOption : CommandOptionsBase
    {
        [Option("no-overwrite", Description = "Specify not to overwrite existing files or content")]
        public bool NoOverwrite { get; set; }
    }
}