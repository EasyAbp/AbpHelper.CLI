using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AbpHelper.Dtos
{
    public class EntityInfo
    {
        public string Namespace { get; }
        public string ClassName { get;  }
        public string? BaseType { get;  }
        public List<PropertyInfo> Properties { get; } = new List<PropertyInfo>();

        public EntityInfo(string @namespace, string className, string? baseType)
        {
            Namespace = @namespace;
            ClassName = className;
            BaseType = baseType;
        }
    }

    public class PropertyInfo
    {
        public string Type { get; }
        public string Name { get; }

        public PropertyInfo(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}