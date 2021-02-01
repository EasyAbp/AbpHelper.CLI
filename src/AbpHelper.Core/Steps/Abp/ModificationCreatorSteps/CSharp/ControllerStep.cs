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
    public class ControllerStep : CSharpModificationCreatorStep
    {
        public ControllerStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var serviceInterfaceInfo = context.GetVariable<TypeInfo>("InterfaceInfo");
            var serviceClassInfo = context.GetVariable<TypeInfo>("ClassInfo");
            var controllerInfo = context.GetVariable<TypeInfo>("ControllerInfo");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);

            // Generate added methods
            var modifications = new List<ModificationBuilder<CSharpSyntaxNode>>();
            var addedMethods = serviceClassInfo.Methods
                .Except(controllerInfo.Methods)                  // Except the existing controller methods
                .Intersect(serviceInterfaceInfo.Methods)        // Only methods defined in the interface need to generate
                                                                // Why not just use the `serviceInterfaceInfo.Methods`? Because we need use attributes info
                                                                // which only defined in the service class
                ;
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