using System.Collections.Generic;
using EasyAbp.AbpHelper.Models;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Extensions
{
    public static class SymbolExtensions
    {
        public static string ToMinimalQualifiedName(this ITypeSymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }
        
        public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol symbol)
        {
            var current = symbol;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }
    }
}