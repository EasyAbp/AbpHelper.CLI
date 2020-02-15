using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Steps.CSharp;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps
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
                    dbContextPropertyText,
                    modifyCondition: root => root.DescendantsNotContain<PropertyDeclarationSyntax>(dbContextPropertyText)
                )
            };
        }
    }
}