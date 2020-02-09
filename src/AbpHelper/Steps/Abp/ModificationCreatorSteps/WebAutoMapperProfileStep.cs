using System.Collections.Generic;
using System.Linq;
using AbpHelper.Extensions;
using AbpHelper.Generator;
using AbpHelper.Models;
using AbpHelper.Steps.CSharp;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AbpHelper.Steps.Abp.ModificationCreatorSteps
{
    public class WebAutoMapperProfileStep : ModificationCreatorStep
    {
        protected override IList<ModificationBuilder> CreateModifications(WorkflowExecutionContext context)
        {
            var entityInfo = context.GetVariable<EntityInfo>("EntityInfo");
            var entityDtoUsingText = context.GetVariable<string>("EntityDtoUsingText");

            return new List<ModificationBuilder>
            {
                new InsertionBuilder(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    entityDtoUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityDtoUsingText)
                ),
                new InsertionBuilder(
                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().GetEndLine(),
                    TextGenerator.GenerateByTemplateName("WebAutoMapperProfile_CreateMap", new {EntityInfo = entityInfo})
                )
            };
        }
    }
}