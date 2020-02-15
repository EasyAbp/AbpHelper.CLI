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
    public class ApplicationAutoMapperProfileStep : ModificationCreatorStep
    {
        protected override IList<ModificationBuilder> CreateModifications(WorkflowExecutionContext context)
        {
            var entityInfo = context.GetVariable<EntityInfo>("EntityInfo");
            string entityUsingText = context.GetVariable<string>("EntityUsingText");
            string entityDtoUsingText = context.GetVariable<string>("EntityDtoUsingText");
            string contents = TextGenerator.GenerateByTemplateName("ApplicationAutoMapperProfile_CreateMap", new {EntityInfo = entityInfo});
            
            return new List<ModificationBuilder>
            {
                new InsertionBuilder(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    entityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    entityDtoUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityDtoUsingText)
                ),
                new InsertionBuilder(
                    root => root.Descendants<ConstructorDeclarationSyntax>().Single().GetEndLine(),
                    contents,
                    modifyCondition: root => root.Descendants<ConstructorDeclarationSyntax>().Single().NotContains(contents)
                )
            };
        }
    }
}