namespace EasyAbp.AbpHelper.Core.Extensions
{
    public static class PathExtensions
    {
        public static string NormalizePath(this string path)
        {
            return path.Replace('\\', '/');
        }
    }
}