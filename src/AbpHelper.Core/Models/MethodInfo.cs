using System;
using System.Collections.Generic;

namespace EasyAbp.AbpHelper.Core.Models
{
    public class MethodInfo : IEquatable<MethodInfo>
    {
        public string Accessor { get; }

        public string ReturnType { get; }
        
        public string FullReturnType { get; }

        public string Name { get; }

        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();
        
        public List<string> Attributes { get; } = new List<string>();

        public MethodInfo(string accessor, string returnType, string fullReturnType, string name)
        {
            Accessor = accessor;
            ReturnType = returnType;
            FullReturnType = fullReturnType;
            Name = name;
        }


        public bool Equals(MethodInfo? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Accessor != other.Accessor || ReturnType != other.ReturnType || FullReturnType != other.FullReturnType || Name != other.Name) return false;
            if (Parameters.Count != other.Parameters.Count) return false;
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (!Parameters[i].Equals(other.Parameters[i])) return false;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MethodInfo) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Accessor, ReturnType, FullReturnType, Name);
        }
    }
}