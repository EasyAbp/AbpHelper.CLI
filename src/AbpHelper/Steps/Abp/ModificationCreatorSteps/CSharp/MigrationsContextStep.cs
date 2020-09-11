using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Workflow;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class MigrationsContextStep : CSharpModificationCreatorStep
    {
        public MigrationsContextStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            var model = context.GetVariable<dynamic>("Model");
            string usingText = TextGenerator.GenerateByTemplateName(templateDir, "MigrationsContext_Using", model);
            string configText = TextGenerator.GenerateByTemplateName(templateDir, "MigrationsContext_ConfigureModule", model);

            MethodDeclarationSyntax GetModelCreatingMethod(CSharpSyntaxNode root)
            {
                return root.Descendants<MethodDeclarationSyntax>().Single(m => m.Identifier.ToString() == "OnModelCreating");
            }

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    usingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(usingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => GetModelCreatingMethod(root).GetEndLine(),
                    configText,
                    modifyCondition: root => GetModelCreatingMethod(root).NotContains(configText)
                ),
            };
        }
    }
}