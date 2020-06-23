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
    public class DbContextInterfaceStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            object model = context.GetVariable<object>("Model");
            string entityUsingText = context.GetVariable<string>("EntityUsingText");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string dbContextUsingText = TextGenerator.GenerateByTemplateName(templateDir, "DbContextInterface_Using", model);
            string dbContextPropertyText = TextGenerator.GenerateByTemplateName(templateDir, "DbContextInterface_Property", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => 1,
                    dbContextUsingText,
                    InsertPosition.Before,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(dbContextUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    entityUsingText,
                    InsertPosition.After,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<InterfaceDeclarationSyntax>().Single().GetEndLine(),
                    dbContextPropertyText,
                    modifyCondition: root => root.DescendantsNotContain<PropertyDeclarationSyntax>(dbContextPropertyText)
                )
            };
        }

        public DbContextInterfaceStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}