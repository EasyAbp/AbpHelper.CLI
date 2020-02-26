using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class DbContextClassStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            var entityUsingText = context.GetVariable<string>("EntityUsingText");
            var dbContextPropertyText = context.GetVariable<string>(TextGenerationStep.DefaultGeneratedTextParameterName);

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
    }
}