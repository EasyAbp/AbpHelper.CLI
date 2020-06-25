using System.Collections.Generic;

namespace EasyAbp.AbpHelper.Models
{
    public class MethodInfo
    {
        public string Accessor { get; }

        public string ReturnType { get; }

        public string Name { get; }

        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();

        public MethodInfo(string accessor, string returnType, string name)
        {
            Accessor = accessor;
            ReturnType = returnType;
            Name = name;
        }
    }
}