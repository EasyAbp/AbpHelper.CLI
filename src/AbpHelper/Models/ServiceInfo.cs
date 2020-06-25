using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace EasyAbp.AbpHelper.Models
{
    public class ServiceInfo
    {
        public string Namespace { get; }
        public string NamespaceLastPart => Namespace.Split('.').Last();
        public string Name { get; }
        public string NamePluralized => Name.Pluralize();
        public string RelativeDirectory { get; }
        
        public List<MethodInfo> Methods { get; } = new List<MethodInfo>();

        public ServiceInfo(string @namespace, string name, string relativeDirectory)
        {
            Namespace = @namespace;
            Name = name;
            RelativeDirectory = relativeDirectory;
        }
    }
}