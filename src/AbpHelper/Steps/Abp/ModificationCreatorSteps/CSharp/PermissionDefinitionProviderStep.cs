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
    public class PermissionDefinitionProviderStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            var model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string permissionDefinitionsText = TextGenerator.GenerateByTemplateName(templateDir, "Permissions_Definitions", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<MethodDeclarationSyntax>().First().GetEndLine(),
                    permissionDefinitionsText,
                    InsertPosition.Before,
                    root => root.DescendantsNotContain<ClassDeclarationSyntax>(permissionDefinitionsText)
                ),
            };
        }
    }
}