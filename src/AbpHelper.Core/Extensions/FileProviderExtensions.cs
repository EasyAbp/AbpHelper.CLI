using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using Volo.Abp.VirtualFileSystem;

namespace EasyAbp.AbpHelper.Core.Extensions
{
    public static class FileProviderExtensions
    {
        public static IEnumerable<(string, IFileInfo)> GetFilesRecursively(this IFileProvider fileProvider, string subpath)
        {
            subpath = subpath.EnsureEndsWith('/');
            var contents = fileProvider.GetDirectoryContents(subpath);
            foreach (var content in contents)
            {
                if (content.IsDirectory)
                {
                    string path = subpath + content.Name;
                    foreach (var file in GetFilesRecursively(fileProvider, path))
                    {
                        yield return file;
                    }
                }
                else
                {
                    yield return (subpath + content.Name, content);
                }
            }
        }

        public static IEnumerable<(string, IFileInfo)> GetFilesRecursively(this IVirtualFileProvider fileProvider, string dir)
        {
            var (virtualDirectory, physicalDirectory) = fileProvider.GetTemplateRootDirectoryMirror(dir);
            var files = GetFilesRecursively((IFileProvider)fileProvider, virtualDirectory);
            if (physicalDirectory == null)
            {
                return files;
            }
            var physicalFiles = new PhysicalFileProvider(physicalDirectory).GetFilesRecursively("").ToList();
            var virtualFiles = files.ToList();
            var physicalPaths = new HashSet<string>(physicalFiles.Select(p => p.Item1));
            return physicalFiles.Concat(virtualFiles.Where(v => !physicalPaths.Contains(v.Item1)));
        }

        /// <summary>
        /// If <paramref name="dir"/> is physical path try to get virtual directory path
        /// </summary>
        /// <param name="dir"></param>
        /// <returns>(virtualDirectory, physicalDirectory)</returns>
        public static (string, string?) GetTemplateRootDirectoryMirror(this IVirtualFileProvider fileProvider, string dir)
        {
            if (dir.StartsWith(AbpHelperCoreConsts.TemplateResourcePathPrefix)) return (dir, null);

            var index = dir.IndexOf(AbpHelperCoreConsts.TemplateResourcePathPrefix);

            if (index == -1) return (dir, null);

            var virtualDirectory = dir.Substring(index);
            var physicalDirectory = dir.Substring(0, index);

            return (virtualDirectory, physicalDirectory);
        }

        /// <summary>
        /// If <paramref name="dir"/> is physical path try to get virtual directory path
        /// </summary>
        /// <param name="dir"></param>
        /// <returns>(virtualDirectory, physicalDirectory)</returns>
        public static (string, string?) GetTemplatePathMirror(this IVirtualFileProvider fileProvider, string dir)
        {
            if (dir.StartsWith(AbpHelperCoreConsts.TemplateResourcePathPrefix)) return (dir, null);

            var index = dir.IndexOf(AbpHelperCoreConsts.TemplateResourcePathPrefix);

            if (index == -1) return (dir, null);

            var virtualPath = dir.Substring(index);

            return (virtualPath, dir);
        }
    }
}