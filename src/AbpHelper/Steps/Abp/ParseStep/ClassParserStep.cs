using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Models;
using Elsa.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ParseStep
{
    public class ClassParserStep : BaseParserStep<ClassDeclarationSyntax>
    {
        public override WorkflowExpression<string> OutputVariableName
        {
            get => GetState(() => new LiteralExpression<string>("ClassInfo"));
            set => SetState(value);
        }

        protected override IEnumerable<MethodInfo> GetMethodInfos(INamedTypeSymbol symbol)
        {
            return symbol
                    .GetBaseTypesAndThis()
                    .SelectMany(type => type.GetMembers())
                    .Where(type => type.Kind == SymbolKind.Method)
                    .Cast<IMethodSymbol>()
                    .Select(SymbolExtensions.ToMethodInfo)
                ;
        }
    }
}