using System.Collections.Generic;
using System.Linq;
using DosSEdo.AbpHelper.Extensions;
using DosSEdo.AbpHelper.Generator;
using DosSEdo.AbpHelper.Models;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class EntityFrameworkCoreModuleStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            object model = context.GetVariable<object>("Model");
            string entityUsingText = context.GetVariable<string>("EntityUsingText");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string contents = TextGenerator.GenerateByTemplateName(templateDir, "EntityFrameworkCoreModule_AddRepository", model);

            CSharpSyntaxNode Func(CSharpSyntaxNode root) => root
                .Descendants<ExpressionStatementSyntax>()
                .Single(node => node.ToString().Contains("AddAbpDbContext"))
            ;

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => 1,
                    entityUsingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(entityUsingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => Func(root).GetEndLine(),
                    contents,
                    modifyCondition: root => Func(root).NotContains(contents)
                ),
            };
        }

        public EntityFrameworkCoreModuleStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}