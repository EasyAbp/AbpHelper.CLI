using System;
using System.IO;
using EasyAbp.AbpHelper.Core.Attributes;
using EasyAbp.AbpHelper.Core.Extensions;

namespace EasyAbp.AbpHelper.Core.Commands
{
    public abstract class CommandOptionsBase
    {
        [Option('d', "directory",
            Description = "The ABP project root directory. If no directory is specified, current directory is used")]
        public virtual string Directory { get; set; } = null!;

        [Option('p', "projectName", Description = "The ABP project name. If no name is provided, last part of project file name is used. Example: project file: 'Acme.BookStore.Domain.csproj', the name will be 'BookStore'")]
        public virtual string ProjectName { get; set; } = null!;

        [Option("exclude", Description = "Exclude directories when searching files, arguments can contain a combination of valid literal path and wildcard (* and ?) characters. Use double asterisk(**) to search all directories. Example: --exclude *Folder1 Folder2/Folder* **/*Folder? **/*Folder*")]
        public virtual string[] Exclude { get; set; } = Array.Empty<string>();

        [Option('t', "template-path", Description = "Prioritize loading template files in the path")]
        public virtual string? TemplatePath { get; set; }

        public virtual string MapTemplatePath(string subPath)
        {
            if (TemplatePath.IsNullOrWhiteSpace())
            {
                return Path.Combine(new[] { "/Templates", subPath }).NormalizePath();
            }

            return Path.Combine(new[] { TemplatePath!, "Templates", subPath }).NormalizePath();
        }
    }
}