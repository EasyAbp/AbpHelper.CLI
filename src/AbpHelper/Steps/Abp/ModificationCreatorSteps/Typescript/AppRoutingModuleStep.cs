using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.Typescript
{
    public class AppRoutingModuleStep : TypeScriptModificationCreatorStep
    {
        protected override IList<ModificationBuilder<IEnumerable<LineNode>>> CreateModifications(
            WorkflowExecutionContext context)
        {
            var model = context.GetVariable<object>("Model");
            var entityInfo = context.GetVariable<EntityInfo>("EntityInfo");
            string importContents =
                TextGenerator.GenerateByTemplateName("AppRoutingModule_ImportApplicationLayoutComponent", model);

            int LineExpression(IEnumerable<LineNode> lines) => lines.Last(l => l.IsMath($"{entityInfo.NamespaceLastPart.ToLower()}")).LineNumber;

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
                    "REPLACE"
                )
            };
        }
    }
}