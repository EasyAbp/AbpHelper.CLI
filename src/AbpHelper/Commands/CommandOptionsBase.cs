using System;
using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands
{
    public abstract class CommandOptionsBase
    {
        [Option('d', "directory", Description = "The ABP project root directory. If no directory is specified, current directory is used")]
        public virtual string Directory { get; set; } = null!;

        [Option("exclude", Description = "Exclude directories when searching files, arguments can contain a combination of valid literal path and wildcard (* and ?) characters. Use double asterisk(**) to search all directories. Example: --exclude *Folder1,Folder2/Folder*,**/*Folder?,**/*Folder*")]
        public virtual string[] ExcludeDirectories { get; set; } = Array.Empty<string>();

        [Option("no-overwrite", Description = "Specify not to overwrite existing files or content")]
        public bool NoOverwrite { get; set; }
    }
}
