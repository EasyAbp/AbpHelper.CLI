using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Models;
using Elsa.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ParseStep
{
    public class InterfaceParserStep : BaseParserStep<InterfaceDeclarationSyntax>
    {
        public override WorkflowExpression<string> OutputVariableName
        {
            get => GetState(() => new LiteralExpression<string>("InterfaceInfo"));
            set => SetState(value);
        }

        protected override IEnumerable<MethodInfo> GetMethodInfos(InterfaceDeclarationSyntax typeDeclarationSyntax, INamedTypeSymbol typeSymbol)
        {
            return typeSymbol
                    .AllInterfaces
                    .Add(typeSymbol)
                    .SelectMany(type => type.GetMembers())
                    .Where(type => type.Kind == SymbolKind.Method)
                    .Cast<IMethodSymbol>()
                    .Select(GetMethodInfoFromSymbol)
                ;
        }
    }
}