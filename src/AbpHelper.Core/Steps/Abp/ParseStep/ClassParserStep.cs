using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ParseStep
{
    [Activity(
        Category = "ClassParserStep",
        Description = "ClassParserStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class ClassParserStep : BaseParserStep<ClassDeclarationSyntax>
    {
        [ActivityInput(
            Hint = "OutputVariableName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public override string? OutputVariableName
        {
            get => GetState<string>(() => "ClassInfo")!;
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