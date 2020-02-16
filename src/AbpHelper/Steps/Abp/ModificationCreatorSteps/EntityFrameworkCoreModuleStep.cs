using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Steps.CSharp;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps
{
    public class EntityFrameworkCoreModuleStep : ModificationCreatorStep
    {
        protected override IList<ModificationBuilder> CreateModifications(WorkflowExecutionContext context)
        {
            var model = context.GetVariable<object>("Model");
            string entityUsingText = context.GetVariable<string>("EntityUsingText");
            string contents = TextGenerator.GenerateByTemplateName("EntityFrameworkCoreModule_AddRepository", model);

            CSharpSyntaxNode Func(CSharpSyntaxNode root) => root
                .Descendants<ExpressionStatementSyntax>()
                .Single(node => node.ToString().Contains("AddAbpDbContext"))
            ;

            return new List<ModificationBuilder>
            {
                new InsertionBuilder(
                    root => 1,
                    entityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder(
                    root => Func(root).GetEndLine(),
                    contents,
                    modifyCondition: root => Func(root).NotContains(contents)
                ),
            };
        }
    }
}