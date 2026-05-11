using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class ApplicationMapperlyStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model");
            string entityUsingText = context.GetVariable<string>("EntityUsingText");
            string entityDtoUsingText = context.GetVariable<string>("EntityDtoUsingText");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            string contents = TextGenerator.GenerateByTemplateName(templateDir, "ApplicationMapperly_CreateMap", model);
            var classNames = CSharpSyntaxTree.ParseText(contents)
                .GetCompilationUnitRoot()
                .Descendants<ClassDeclarationSyntax>()
                .Select(s => s.Identifier.ValueText);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    entityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    entityDtoUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityDtoUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.GetEndLine()-1,
                    contents,
                    InsertPosition.After
                )
            };
        }

        public ApplicationMapperlyStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}