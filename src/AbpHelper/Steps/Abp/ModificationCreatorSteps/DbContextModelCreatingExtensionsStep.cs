using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
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
            var model = context.GetVariable<object>("Model");
            var entityUsingText = context.GetVariable<string>("EntityUsingText");
            var modelingUsingText = TextGenerator.GenerateByTemplateName("DbContextModelCreatingExtensions_Using", model);
            var entityConfigText = TextGenerator.GenerateByTemplateName("DbContextModelCreatingExtensions_EntityConfig", model);

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
                    entityConfigText,
                    modifyCondition: root => root.Descendants<MethodDeclarationSyntax>().First().NotContains(entityConfigText)
                )
            };
        }
    }
}