using System;
using System.IO;
using EasyAbp.AbpHelper.Core.Attributes;

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

        [Option("templates-path", Description = "Config templates path, Built-in templates are used by default")]
        public string? TemplatesPath { set { _templatesPath = value; } }

        private string? _templatesPath = string.Empty;

        public string TemplatesPathCombine(string subPath)
        {
            if (_templatesPath.IsNullOrWhiteSpace())
            {
                return Path.Combine(new[] { AppDomain.CurrentDomain.BaseDirectory, "Templates", subPath });
            }

            return Path.Combine(_templatesPath!, subPath);
        }
    }
}