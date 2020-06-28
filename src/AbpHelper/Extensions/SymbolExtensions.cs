using System.Collections.Generic;
using System.CommandLine;
using Microsoft.CodeAnalysis;

namespace EasyAbp.AbpHelper.Extensions
{
    public static class SymbolExtensions
    {
        public static string ToMinimalQualifiedName(this ITypeSymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }
        
        public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol? type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }
    }
}