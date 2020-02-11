using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.CSharp;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps
{
    public class DbContextModelCreatingExtensionsStep : ModificationCreatorStep
    {
        protected override IList<ModificationBuilder> CreateModifications(WorkflowExecutionContext context)
        {
            var entityUsingText = context.GetVariable<string>("EntityUsingText");
            var modelingUsingText = context.GetVariable<string>("ModelingUsingText");
            var entityConfigText = context.GetVariable<string>("EntityConfigText");

            return new List<ModificationBuilder>
            {
                new InsertionBuilder(
                    root => 1,
                    entityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    modelingUsingText,
                    InsertPosition.After,
                    root => root.DescendantsNotContain<UsingDirectiveSyntax>(modelingUsingText)),
                new InsertionBuilder(
                    root => root.Descendants<MethodDeclarationSyntax>().First().GetEndLine(),
                    entityConfigText
                )
            };
        }
    }
}