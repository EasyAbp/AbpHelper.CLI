using System.Collections.Generic;
using System.Linq;
using AbpHelper.Extensions;
using AbpHelper.Models;
using AbpHelper.Steps.Common;
using AbpHelper.Steps.CSharp;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AbpHelper.Steps.Abp.ModificationCreatorSteps
{
    public class DbContextStep : ModificationCreatorStep
    {
        protected override IList<ModificationBuilder> CreateModifications(WorkflowExecutionContext context)
        {
            var entityUsingText = context.GetVariable<string>("EntityUsingText");
            var dbContextPropertyText = context.GetVariable<string>(TextGenerationStep.DefaultGeneratedTextParameterName);

            return new List<ModificationBuilder>
            {
                new InsertionBuilder(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    entityUsingText,
                    InsertPosition.After,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder(
                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().Identifier.GetStartLine() - 1,
                    dbContextPropertyText
                )
            };
        }
    }
}