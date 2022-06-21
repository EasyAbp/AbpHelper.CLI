using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    [Activity(
        Category = "ControllerStep",
        Description = "ControllerStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class ControllerStep : CSharpModificationCreatorStep
    {
        [ActivityInput(
            Hint = "InterfaceInfo",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public TypeInfo InterfaceInfo
        {
            get => GetState<TypeInfo>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "ClassInfo",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public TypeInfo ClassInfo
        {
            get => GetState<TypeInfo>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "ControllerInfo",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public TypeInfo ControllerInfo
        {
            get => GetState<TypeInfo>()!;
            set => SetState(value);
        }

        public ControllerStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            // Generate added methods
            var modifications = new List<ModificationBuilder<CSharpSyntaxNode>>();
            var addedMethods = ClassInfo.Methods
                    .Except(ControllerInfo.Methods) // Except the existing controller methods
                    .Intersect(InterfaceInfo.Methods) // Only methods defined in the interface need to generate
                // Why not just use the `serviceInterfaceInfo.Methods`? Because we need use attributes info
                // which only defined in the service class
                ;
            foreach (var method in addedMethods)
            {
                var model = new { method };
                string methodText = TextGenerator.GenerateByTemplateName(TemplateDirectory, "ControllerMethod", model);
                modifications.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => root.Descendants<ClassDeclarationSyntax>().First().GetEndLine(),
                        methodText
                    ));
            }

            return modifications;
        }
    }
}