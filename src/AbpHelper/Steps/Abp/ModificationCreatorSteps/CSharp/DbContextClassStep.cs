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
    public class DbContextClassStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            object model = context.GetVariable<object>("Model");
            string entityUsingText = context.GetVariable<string>("EntityUsingText");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string dbContextPropertyText = TextGenerator.GenerateByTemplateName(templateDir, "DbContextClass_Property", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    entityUsingText,
                    InsertPosition.After,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().Identifier.GetStartLine() - 1,
                    dbContextPropertyText,
                    modifyCondition: root => root.DescendantsNotContain<PropertyDeclarationSyntax>(dbContextPropertyText)
                )
            };
        }

        public DbContextClassStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}