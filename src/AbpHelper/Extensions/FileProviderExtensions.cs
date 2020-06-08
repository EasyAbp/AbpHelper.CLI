using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

namespace EasyAbp.AbpHelper.Extensions
{
    public static class FileProviderExtensions
    {
        public static IEnumerable<IFileInfo> GetFilesRecursively(this IFileProvider fileProvider, string subpath)
        {
            var contents = fileProvider.GetDirectoryContents(subpath);
            foreach (var content in contents)
            {
                if (content.IsDirectory)
                {
                    foreach (var file in GetFilesRecursively(fileProvider, subpath + "/" + content.Name))
                    {
                        yield return file;
                    }
                }
                else
                {
                    yield return content;
                }
            }
        }

        /// <summary>
        /// See: https://github.com/abpframework/abp/issues/4248
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static string GetCorrectPath(this IFileInfo fileInfo)
        {
            string path = fileInfo.GetVirtualOrPhysicalPathOrNull();
            return Regex.Replace(path, "__(.+?)__", match => @"{{" + match.Value.Replace('/', '.').Replace("_", "") + @"}}");
        }
    }
}