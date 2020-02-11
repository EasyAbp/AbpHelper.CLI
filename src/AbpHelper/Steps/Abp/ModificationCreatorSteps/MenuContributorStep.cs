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
    public class MenuContributorStep : ModificationCreatorStep
    {
        protected override IList<ModificationBuilder> CreateModifications(WorkflowExecutionContext context)
        {
            var entityInfo = context.GetVariable<EntityInfo>("EntityInfo");

            return new List<ModificationBuilder>
            {
                new InsertionBuilder(root => root.Descendants<MethodDeclarationSyntax>()
                        .Single(n => n.Identifier.ToString() == "ConfigureMainMenuAsync")
                        .GetEndLine(),
                    TextGenerator.GenerateByTemplateName("MenuContributor_AddMenuItem", new {EntityInfo = entityInfo})
                )
            };
        }
    }
}