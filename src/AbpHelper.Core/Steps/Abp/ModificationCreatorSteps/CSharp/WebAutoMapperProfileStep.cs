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
        Category = "WebAutoMapperProfileStep",
        Description = "WebAutoMapperProfileStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class WebAutoMapperProfileStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model")!;
            var usingText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "WebAutoMapperProfile_Using", model);
            var contents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "WebAutoMapperProfile_CreateMap", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    usingText,
                    modifyCondition: root => root.NotContains(usingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().GetEndLine(),
                    contents,
                    modifyCondition: root =>
                        root.Descendants<ConstructorDeclarationSyntax>().Single().NotContains(contents)
                )
            };
        }

        public WebAutoMapperProfileStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}