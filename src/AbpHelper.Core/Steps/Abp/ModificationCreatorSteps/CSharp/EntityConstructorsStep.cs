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
        Category = "EntityConstructorsStep",
        Description = "EntityConstructorsStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class EntityConstructorsStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model")!;
            var protectedCtorText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "Entity_ProtectedConstructor", model);
            var publicCtorText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "Entity_PublicConstructor", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ClassDeclarationSyntax>().Single().GetEndLine(),
                    protectedCtorText,
                    InsertPosition.Before,
                    root => root.DescendantsNotContain<ClassDeclarationSyntax>(protectedCtorText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ClassDeclarationSyntax>().Single().GetEndLine(),
                    publicCtorText,
                    InsertPosition.Before,
                    modifyCondition: root => root.DescendantsNotContain<ClassDeclarationSyntax>(publicCtorText)
                )
            };
        }

        public EntityConstructorsStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}