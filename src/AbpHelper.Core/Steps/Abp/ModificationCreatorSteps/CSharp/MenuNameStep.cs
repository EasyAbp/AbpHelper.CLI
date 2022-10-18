using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.Attributes;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    [Activity(
        Category = "MenuNameStep",
        Description = "MenuNameStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class MenuNameStep : CSharpModificationCreatorStep
    {
        public MenuNameStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var addMenuNameText = TextGenerator.GenerateByTemplateName(TemplateDirectory!, "Menus_AddMenuName",
                context.GetVariable<object>("Model")!);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ClassDeclarationSyntax>().Single().GetEndLine(),
                    addMenuNameText,
                    modifyCondition: root => root.DescendantsNotContain<ClassDeclarationSyntax>(addMenuNameText)
                ),
            };
        }
    }
}