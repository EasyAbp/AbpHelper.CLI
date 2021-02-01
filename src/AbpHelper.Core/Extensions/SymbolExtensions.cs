using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EasyAbp.AbpHelper.Core.Extensions
{
    public static class SymbolExtensions
    {
        public static string ToFullName(this ITypeSymbol symbol)
        {
            string fullName;

            switch (symbol.SpecialType)
            {
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                case SpecialType.System_DateTime:
                    fullName = $"{symbol.ContainingNamespace.Name}.{symbol.MetadataName}";
                    break;
                default:
                    fullName = symbol.ToDisplayString();
                    break;
            }

            return fullName;
        }

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