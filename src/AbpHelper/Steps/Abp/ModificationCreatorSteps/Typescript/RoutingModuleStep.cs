using System.Collections.Generic;
using System.Linq;
using DosSEdo.AbpHelper.Generator;
using DosSEdo.AbpHelper.Models;
using Elsa.Services.Models;
using JetBrains.Annotations;

namespace DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.Typescript
{
    public class RoutingModuleStep : TypeScriptModificationCreatorStep
    {
        protected override IList<ModificationBuilder<IEnumerable<LineNode>>> CreateModifications(
            WorkflowExecutionContext context)
        {
            object model = context.GetVariable<object>("Model");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
            string importContents = TextGenerator.GenerateByTemplateName(templateDir, "RoutingModule_ImportList", model);
            string moduleContents = TextGenerator.GenerateByTemplateName(templateDir, "RoutingModule_Routes", model);

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

        public RoutingModuleStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}