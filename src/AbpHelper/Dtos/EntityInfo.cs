using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AbpHelper.Dtos
{
    public class EntityInfo
    {
        public string Namespace { get; }
        public string ClassName { get;  }
        public string BaseType { get;  }
        public ICollection<PropertyInfo> Properties { get; } = new Collection<PropertyInfo>();

        public EntityInfo(string ns, string className, string baseType)
        {
            Namespace = ns;
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