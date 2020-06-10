﻿using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class EntityConstructorsStep : CSharpModificationCreatorStep
    {
        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            var model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string protectedCtorText = TextGenerator.GenerateByTemplateName(templateDir, "Entity_ProtectedConstructor", model);
            string publicCtorText = TextGenerator.GenerateByTemplateName(templateDir, "Entity_PublicConstructor", model);

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

        public EntityConstructorsStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}