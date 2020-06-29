using System.Collections.Generic;
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
    public class ControllerStep : CSharpModificationCreatorStep
    {
        public ControllerStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            var serviceInfo = context.GetVariable<ClassInfo>("ServiceInfo");
            var controllerInfo = context.GetVariable<ClassInfo>("ControllerInfo");
            string templateDir = context.GetVariable<string>("TemplateDirectory");

            // Generate added methods
            var modifications = new List<ModificationBuilder<CSharpSyntaxNode>>();
            var addedMethods = serviceInfo.Methods.Except(controllerInfo.Methods);
            foreach (var method in addedMethods)
            {
                var model = new {method};
                string methodText = TextGenerator.GenerateByTemplateName(templateDir, "ControllerMethod", model);
                modifications.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ClassDeclarationSyntax>().First().GetEndLine(),
                    methodText
                ));
            }

            return modifications;
        }
    }
}