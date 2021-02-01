using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class AppServiceClassStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            string usingTaskContents = TextGenerator.GenerateByTemplateName(templateDir, "AppService_UsingTask", model);
            string usingDtoContents = TextGenerator.GenerateByTemplateName(templateDir, "AppService_UsingDto", model);
            string classContents = TextGenerator.GenerateByTemplateName(templateDir, "AppServiceClass", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => 1,
                    usingTaskContents,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(usingTaskContents)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => 1,
                    usingDtoContents,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(usingDtoContents)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ClassDeclarationSyntax>().Single().GetEndLine(),
                    classContents
                ),
            };
        }

        public AppServiceClassStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}