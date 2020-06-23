using System.Collections.Generic;
using System.Linq;
using DosSEdo.AbpHelper.Generator;
using DosSEdo.AbpHelper.Models;
using Elsa.Services.Models;
using JetBrains.Annotations;

namespace DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.Typescript
{
    public class ModuleStep : TypeScriptModificationCreatorStep
    {
        protected override IList<ModificationBuilder<IEnumerable<LineNode>>> CreateModifications(
            WorkflowExecutionContext context)
        {
            object model = context.GetVariable<object>("Model");
            EntityInfo entityInfo = context.GetVariable<EntityInfo>("EntityInfo");
            string templateDir = context.GetVariable<string>("TemplateDirectory");
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