using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Models;
using Elsa.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ParseStep
{
    public class ClassParserStep : BaseParserStep<ClassDeclarationSyntax>
    {
        public override WorkflowExpression<string> OutputVariableName
        {
            get => GetState(() => new LiteralExpression<string>("ClassInfo"));
            set => SetState(value);
        }

        protected override IEnumerable<MethodInfo> GetMethodInfos(ClassDeclarationSyntax typeDeclarationSyntax, INamedTypeSymbol typeSymbol)
        {
            return typeSymbol
                    .GetBaseTypesAndThis()
                    .SelectMany(type => type.GetMembers())
                    .OfType<IMethodSymbol>()
                    .Select(GetMethodInfoFromSymbol)
                ;
        }
    }
}