using System;
using System.Collections.Generic;

namespace EasyAbp.AbpHelper.Models
{
    public class ParameterInfo : IEquatable<ParameterInfo>
    {
        public string Type { get; }

        public string FullType { get; }

        public string Name { get; }

        public ParameterInfo(string type, string fullType, string name)
        {
            Type = type;
            FullType = fullType;
            Name = name;
        }

        public bool Equals(ParameterInfo? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && FullType == other.FullType && Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParameterInfo) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, FullType, Name);
        }
    }
}