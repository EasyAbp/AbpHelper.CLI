using System.IO;
using EasyAbp.AbpHelper.Core.Attributes;
using NUglify.Helpers;

namespace EasyAbp.AbpHelper.Core.Commands.Generate
{
    public class GenerateCommandOption : CommandOptionsBase
    {
        protected virtual string OverwriteVariableName => CommandConsts.OverwriteVariableName;

        [Option("no-overwrite", Description = "Specify not to overwrite existing files or content")]
        public bool NoOverwrite { get; set; }

        [Option("templates-path", Description = "Config templates path, Built-in templates are used by default")]
        public string? TemplatesPath { get; set; }

        public string GetTemplatesPath(string subPath)
        {
            if (TemplatesPath.IsNullOrWhiteSpace())
            {
                return "/Templates/" + subPath;
            }

            return Path.Combine(TemplatesPath!, subPath);
        }
    }
}