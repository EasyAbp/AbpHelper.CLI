using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.CSharp;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps
{
    public class MenuContributorStep : ModificationCreatorStep
    {
        protected override IList<ModificationBuilder> CreateModifications(WorkflowExecutionContext context)
        {
            var entityInfo = context.GetVariable<EntityInfo>("EntityInfo");
            string contents = TextGenerator.GenerateByTemplateName("MenuContributor_AddMenuItem", new {EntityInfo = entityInfo});

            CSharpSyntaxNode Func(CSharpSyntaxNode root) => root.Descendants<MethodDeclarationSyntax>()
                    .Single(n => n.Identifier.ToString() == "ConfigureMainMenuAsync");

            return new List<ModificationBuilder>
            {
                new InsertionBuilder(root => Func(root).GetEndLine(),
                    contents,
                    modifyCondition: root => Func(root).NotContains(contents)
                )
            };
        }
    }
}