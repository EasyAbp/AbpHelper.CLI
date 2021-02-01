using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class WebAutoMapperProfileStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);

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