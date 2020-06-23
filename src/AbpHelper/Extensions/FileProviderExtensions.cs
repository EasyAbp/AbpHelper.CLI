using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Volo.Abp;

namespace DosSEdo.AbpHelper.Extensions
{
    public static class FileProviderExtensions
    {
        public static IEnumerable<(string, IFileInfo)> GetFilesRecursively(this IFileProvider fileProvider, string subpath)
        {
            subpath = subpath.EnsureEndsWith('/');
            IDirectoryContents contents = fileProvider.GetDirectoryContents(subpath);
            foreach (IFileInfo content in contents)
            {
                if (content.IsDirectory)
                {
                    string path = subpath + content.Name;
                    foreach ((string, IFileInfo) file in GetFilesRecursively(fileProvider, path))
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

        /// <summary>
        /// Reads file content as string using <see cref="Encoding.UTF8"/> encoding.
        /// </summary>
        public static string ReadAsString([NotNull] this IFileInfo fileInfo)
        {
            return fileInfo.ReadAsString(Encoding.UTF8);
        }

        /// <summary>
        /// Reads file content as string using <see cref="Encoding.UTF8"/> encoding.
        /// </summary>
        public static Task<string> ReadAsStringAsync([NotNull] this IFileInfo fileInfo)
        {
            return fileInfo.ReadAsStringAsync(Encoding.UTF8);
        }

        /// <summary>
        /// Reads file content as string using the given <paramref name="encoding"/>.
        /// </summary>
        public static string ReadAsString([NotNull] this IFileInfo fileInfo, Encoding encoding)
        {
            Check.NotNull(fileInfo, nameof(fileInfo));

            using (Stream stream = fileInfo.CreateReadStream())
            {
                using (StreamReader streamReader = new StreamReader(stream, encoding, true))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Reads file content as string using the given <paramref name="encoding"/>.
        /// </summary>
        public static async Task<string> ReadAsStringAsync([NotNull] this IFileInfo fileInfo, Encoding encoding)
        {
            Check.NotNull(fileInfo, nameof(fileInfo));

            using (Stream stream = fileInfo.CreateReadStream())
            {
                using (StreamReader streamReader = new StreamReader(stream, encoding, true))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }
    }
}