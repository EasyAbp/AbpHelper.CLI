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
    public class DbContextModelCreatingExtensionsStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model");
            var entityUsingText = context.GetVariable<string>("EntityUsingText");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            var modelingUsingText = TextGenerator.GenerateByTemplateName(templateDir, "DbContextModelCreatingExtensions_Using", model);
            var entityConfigText = TextGenerator.GenerateByTemplateName(templateDir, "DbContextModelCreatingExtensions_EntityConfig", model);

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