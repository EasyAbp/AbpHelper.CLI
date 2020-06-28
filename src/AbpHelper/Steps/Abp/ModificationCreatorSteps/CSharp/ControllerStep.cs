using System.Collections.Generic;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using Elsa.Results;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class ControllerStep : CSharpModificationCreatorStep
    {
        public ControllerStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context)
        {
            var projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");
            var serviceInfo = context.GetVariable<ServiceInfo>("ServiceInfo");
            var model = context.GetVariable<object>("Model");
            
            return new List<ModificationBuilder<CSharpSyntaxNode>>();
        }
    }
}