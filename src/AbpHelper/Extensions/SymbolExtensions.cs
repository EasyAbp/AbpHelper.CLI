using System.Collections.Generic;
using EasyAbp.AbpHelper.Models;
using Microsoft.CodeAnalysis;
using System.Linq;

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

        public static MethodInfo ToMethodInfo(this IMethodSymbol symbol)
        {
            var methodInfo = new MethodInfo(
                symbol.DeclaredAccessibility.ToString().ToLower(),
                symbol.ReturnType.ToMinimalQualifiedName(),
                symbol.ReturnType.ToDisplayString(),
                symbol.Name
            );
            methodInfo.Parameters.AddRange(
                symbol.Parameters
                    .Select(ps => new ParameterInfo(
                        ps.Type.ToMinimalQualifiedName(),
                        ps.Type.ToDisplayString(),
                        ps.Name)
                    )
            );
            return methodInfo;
        }
    }
}