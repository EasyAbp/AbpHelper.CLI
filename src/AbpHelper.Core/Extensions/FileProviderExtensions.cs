using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;

namespace EasyAbp.AbpHelper.Core.Extensions
{
    public static class FileProviderExtensions
    {
        public static IEnumerable<(string, IFileInfo)> GetFilesRecursively(this IFileProvider fileProvider, string subpath)
        {
            subpath = subpath.EnsureEndsWith('/');

            var contents = Directory.Exists(subpath) ?
                new PhysicalFileProvider(subpath).GetDirectoryContents("") : fileProvider.GetDirectoryContents(subpath);

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
    }
}