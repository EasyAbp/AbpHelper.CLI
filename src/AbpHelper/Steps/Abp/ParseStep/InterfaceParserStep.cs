using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ParseStep
{
    public class InterfaceParserStep : BaseParserStep<InterfaceDeclarationSyntax>
    {
        protected override string GetOutputVariableName()
        {
            return "InterfaceInfo";
        }

        protected override IEnumerable<MethodInfo> GetMethodInfos(INamedTypeSymbol symbol)
        {
            return symbol
                    .AllInterfaces
                    .Add(symbol)
                    .SelectMany(type => type.GetMembers())
                    .Where(type => type.Kind == SymbolKind.Method)
                    .Cast<IMethodSymbol>()
                    .Select(SymbolExtensions.ToMethodInfo)
                ;
        }
    }
}