using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace DosSEdo.AbpHelper.Models
{
    public class EntityInfo
    {
        public EntityInfo(string @namespace, string name, string? baseType, string? primaryKey, string relativeDirectory)
        {
            Namespace = @namespace;
            Name = name;
            BaseType = baseType;
            PrimaryKey = primaryKey;
            RelativeDirectory = relativeDirectory.Replace('\\', '/');
        }

        public string Namespace { get; }
        public string RelativeNamespace => RelativeDirectory.Replace('/', '.');
        public string RelativeDirectory { get; }
        public string NamespaceLastPart => Namespace.Split('.').Last();
        public string Name { get; }
        public string NamePluralized => Name.Pluralize();
        public string? BaseType { get; }
        public string? PrimaryKey { get; }
        public List<PropertyInfo> Properties { get; } = new List<PropertyInfo>();
        public string? CompositeKeyName { get; set; }
        public List<PropertyInfo> CompositeKeys { get; } = new List<PropertyInfo>();
    }

    public class PropertyInfo
    {
        public PropertyInfo(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Type { get; }
        public string Name { get; }
    }
}