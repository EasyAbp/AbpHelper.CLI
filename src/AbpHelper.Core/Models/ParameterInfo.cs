using System;
using EasyAbp.AbpHelper.Core.Extensions;
using Microsoft.CodeAnalysis;

namespace EasyAbp.AbpHelper.Core.Models
{
    public class ParameterInfo : IEquatable<ParameterInfo>
    {
        public string Type => ParameterSymbol.Type.ToMinimalQualifiedName();

        public string FullType => ParameterSymbol.Type.ToFullName();

        public string Name => ParameterSymbol.Name;

        public IParameterSymbol ParameterSymbol { get; }

        public ParameterInfo(IParameterSymbol parameterSymbol)
        {
            ParameterSymbol = parameterSymbol;
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