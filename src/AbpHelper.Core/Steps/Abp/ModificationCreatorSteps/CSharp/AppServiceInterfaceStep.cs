using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.Attributes;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    [Activity(
        Category = "AppServiceInterfaceStep",
        Description = "AppServiceInterfaceStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class AppServiceInterfaceStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model")!;
            var usingTaskContents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "AppService_UsingTask", model);
            var usingDtoContents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "AppService_UsingDto", model);
            var interfaceContents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "AppServiceInterface", model);

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
                    root => root.Descendants<InterfaceDeclarationSyntax>().Single().GetEndLine(),
                    interfaceContents
                ),
            };
        }

        public AppServiceInterfaceStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}