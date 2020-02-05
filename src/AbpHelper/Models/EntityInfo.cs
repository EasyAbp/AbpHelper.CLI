using System.Collections.Generic;
using Humanizer;

namespace AbpHelper.Models
{
    public class EntityInfo
    {
        public EntityInfo(string @namespace, string name, string? baseType, string? primaryKey)
        {
            Namespace = @namespace;
            Name = name;
            BaseType = baseType;
            PrimaryKey = primaryKey;
        }

        public string Namespace { get; }
        public string Name { get; }
        public string NamePluralized => Name.Pluralize();
        public string? BaseType { get; }
        public string? PrimaryKey { get; set; }
        public List<PropertyInfo> Properties { get; } = new List<PropertyInfo>();
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