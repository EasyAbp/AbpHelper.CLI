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
    public class PermissionsStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            object model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string permissionNamesText = TextGenerator.GenerateByTemplateName(templateDir, "Permissions_Names", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ClassDeclarationSyntax>().First().GetEndLine(),
                    permissionNamesText,
                    InsertPosition.Before,
                    root => root.DescendantsNotContain<ClassDeclarationSyntax>(permissionNamesText)
                ),
            };
        }

        public PermissionsStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}