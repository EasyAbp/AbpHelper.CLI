using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Models;
using Elsa.Expressions;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ParseStep
{
    public class InterfaceParserStep : BaseParserStep<InterfaceDeclarationSyntax>
    {
        public override WorkflowExpression<string> OutputVariableName
        {
            get => GetState(() => new LiteralExpression<string>("InterfaceInfo"));
            set => SetState(value);
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