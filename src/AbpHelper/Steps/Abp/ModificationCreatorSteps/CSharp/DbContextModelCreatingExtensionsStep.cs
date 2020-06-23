using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class DbContextModelCreatingExtensionsStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            object model = context.GetVariable<object>("Model");
            string entityUsingText = context.GetVariable<string>("EntityUsingText");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string modelingUsingText = TextGenerator.GenerateByTemplateName(templateDir, "DbContextModelCreatingExtensions_Using", model);
            string entityConfigText = TextGenerator.GenerateByTemplateName(templateDir, "DbContextModelCreatingExtensions_EntityConfig", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => 1,
                    entityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    modelingUsingText,
                    InsertPosition.After,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(modelingUsingText)),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<MethodDeclarationSyntax>().First().GetEndLine(),
                    entityConfigText,
                    modifyCondition: root => root.Descendants<MethodDeclarationSyntax>().First().NotContains(entityConfigText)
                )
            };
        }

        public DbContextModelCreatingExtensionsStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}