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
    }
}