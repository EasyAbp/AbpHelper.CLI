using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.Services.Models;
using JetBrains.Annotations;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.Typescript
{
    public class ModuleStep : TypeScriptModificationCreatorStep
    {
        protected override IList<ModificationBuilder<IEnumerable<LineNode>>> CreateModifications(
            WorkflowExecutionContext context)
        {
            var model = context.GetVariable<object>("Model");
            var entityInfo = context.GetVariable<EntityInfo>("EntityInfo");
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            string importContents = TextGenerator.GenerateByTemplateName(templateDir, "Module_ImportSharedModule", model);
            string sharedModuleContents = TextGenerator.GenerateByTemplateName(templateDir, "Module_SharedModule", model);

            int LineExpression(IEnumerable<LineNode> lines) => lines.Last(l => l.IsMath($"{entityInfo.NamespaceLastPart}RoutingModule")).LineNumber;

            return new List<ModificationBuilder<IEnumerable<LineNode>>>
            {
                new InsertionBuilder<IEnumerable<LineNode>>(
                    lines => lines.Last(l => l.IsMath("^import")).LineNumber,
                    importContents,
                    InsertPosition.After,
                    lines => lines.Where(l => l.IsMath("^import")).All(l => !l.LineContent.Contains(importContents))
                ),
                new InsertionBuilder<IEnumerable<LineNode>>(
                    LineExpression,
                    sharedModuleContents,
                    InsertPosition.After,
                    lines => lines.All(l => !l.LineContent.Contains(sharedModuleContents))
                )
            };
        }

        public ModuleStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}