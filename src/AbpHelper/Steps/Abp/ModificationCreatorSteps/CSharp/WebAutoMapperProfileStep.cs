using System.Collections.Generic;
using System.Linq;
using DosSEdo.AbpHelper.Extensions;
using DosSEdo.AbpHelper.Generator;
using DosSEdo.AbpHelper.Models;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class WebAutoMapperProfileStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            object model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>("TemplateDirectory");

            string usingText = TextGenerator.GenerateByTemplateName(templateDir, "WebAutoMapperProfile_Using", model);

            string contents = TextGenerator.GenerateByTemplateName(templateDir, "WebAutoMapperProfile_CreateMap", model);
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
                    modifyCondition: root => root.Descendants<ConstructorDeclarationSyntax>().Single().NotContains(contents)
                )
            };
        }

        public WebAutoMapperProfileStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}