using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.Attributes;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.Typescript
{
    [Activity(
        Category = "RoutingModuleStep",
        Description = "RoutingModuleStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class RoutingModuleStep : TypeScriptModificationCreatorStep
    {
        protected override IList<ModificationBuilder<IEnumerable<LineNode>>> CreateModifications(
            ActivityExecutionContext context)
        {
            var model = context.GetVariable<object>("Model")!;
            var importContents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "RoutingModule_ImportList", model);
            var moduleContents = TextGenerator.GenerateByTemplateName(TemplateDirectory, "RoutingModule_Routes", model);

            int LineExpression(IEnumerable<LineNode> lines) => lines.Last(l => l.IsMath($"const routes")).LineNumber;

            return new List<ModificationBuilder<IEnumerable<LineNode>>>
            {
                new InsertionBuilder<IEnumerable<LineNode>>(
                    lines => lines.Last(l => l.IsMath("^import")).LineNumber,
                    importContents,
                    InsertPosition.After,
                    lines => lines.Where(l => l.IsMath("^import")).All(l => !l.LineContent.Contains(importContents))
                ),
                new ReplacementBuilder<IEnumerable<LineNode>>(
                    LineExpression,
                    LineExpression,
                    moduleContents
                )
            };
        }

        public RoutingModuleStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}