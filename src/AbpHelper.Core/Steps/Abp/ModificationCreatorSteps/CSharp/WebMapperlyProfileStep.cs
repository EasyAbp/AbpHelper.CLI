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
    public class WebMapperlyProfileStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);

            string usingText = TextGenerator.GenerateByTemplateName(templateDir, "WebAutoMapperProfile_Using", model);

            string contents = TextGenerator.GenerateByTemplateName(templateDir, "WebMapperly_CreateMap", model);
            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    usingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(usingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.GetEndLine()-1,
                    contents,
                    InsertPosition.After
                )
            };
        }

        public WebMapperlyProfileStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}