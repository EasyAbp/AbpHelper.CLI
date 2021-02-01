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
    public class DbContextInterfaceStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model");
            string entityUsingText = context.GetVariable<string>("EntityUsingText");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
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