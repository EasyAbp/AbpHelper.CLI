using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class MenuContributorStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            var model = context.GetVariable<object>("Model");
            string contents = TextGenerator.GenerateByTemplateName("MenuContributor_AddMenuItem", model);

            CSharpSyntaxNode Func(CSharpSyntaxNode root) => root.Descendants<MethodDeclarationSyntax>()
                    .Single(n => n.Identifier.ToString() == "ConfigureMainMenuAsync");

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(root => Func(root).GetEndLine(),
                    contents,
                    modifyCondition: root => Func(root).NotContains(contents)
                )
            };
        }
    }
}