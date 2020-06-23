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
    public class PermissionDefinitionProviderStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            ProjectInfo projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");
            object model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string permissionDefinitionsText = TextGenerator.GenerateByTemplateName(templateDir, "Permissions_Definitions", model);

            List<ModificationBuilder<CSharpSyntaxNode>> builders = new List<ModificationBuilder<CSharpSyntaxNode>>();

            builders.Add(new InsertionBuilder<CSharpSyntaxNode>(
                root => root.Descendants<MethodDeclarationSyntax>().First().GetEndLine(),
                permissionDefinitionsText,
                InsertPosition.Before,
                root => root.DescendantsNotContain<ClassDeclarationSyntax>(permissionDefinitionsText)
            ));

            if (projectInfo.TemplateType == TemplateType.Application)
            {
                // Noting special to do
            }
            else if (projectInfo.TemplateType == TemplateType.Module)
            {
                string addGroupText = TextGenerator.GenerateByTemplateName(templateDir, "Permissions_AddGroup", model);

                // Uncomment the add group statement
                builders.Add(new ReplacementBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<MethodDeclarationSyntax>().First().GetStartLine() + 2,
                    root => root.Descendants<MethodDeclarationSyntax>().First().GetStartLine() + 2,
                    addGroupText,
                    modifyCondition: root => !root.DescendantsNotContain<MethodDeclarationSyntax>($"//" + addGroupText)
                ));
            }

            return builders;
        }

        public PermissionDefinitionProviderStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}