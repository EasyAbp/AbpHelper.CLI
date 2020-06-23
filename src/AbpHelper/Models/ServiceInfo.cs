using System.Linq;
using Humanizer;

namespace DosSEdo.AbpHelper.Models
{
    public class ServiceInfo
    {
        public string Namespace { get; }
        public string NamespaceLastPart => Namespace.Split('.').Last();
        public string Name { get; }
        public string NamePluralized => Name.Pluralize();
        public int MethodsCount { get; }
        public string RelativeDirectory { get; }

        public ServiceInfo(string @namespace, string name, int methodsCount, string relativeDirectory)
        {
            Namespace = @namespace;
            Name = name;
            MethodsCount = methodsCount;
            RelativeDirectory = relativeDirectory;
        }
    }
}