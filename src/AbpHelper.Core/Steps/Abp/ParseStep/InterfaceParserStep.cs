using System.Collections.Generic;
using System.Linq;
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
        Category = "InterfaceParserStep",
        Description = "InterfaceParserStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class InterfaceParserStep : BaseParserStep<InterfaceDeclarationSyntax>
    {
        [ActivityInput(
            Hint = "OutputVariableName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public override string OutputVariableName
        {
            get => GetState<string>(() => "InterfaceInfo");
            set => SetState(value);
        }

        protected override IEnumerable<MethodInfo> GetMethodInfos(InterfaceDeclarationSyntax typeDeclarationSyntax,
            INamedTypeSymbol typeSymbol)
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