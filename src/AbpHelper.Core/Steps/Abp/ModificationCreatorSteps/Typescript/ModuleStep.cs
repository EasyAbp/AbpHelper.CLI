using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.Typescript
{
    [Activity(
        Category = "ModuleStep",
        Description = "ModuleStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class ModuleStep : TypeScriptModificationCreatorStep
    {
        [ActivityInput(
            Hint = "EntityInfo",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public EntityInfo EntityInfo
        {
            get => GetState<EntityInfo>()!;
            set => SetState(value);
        }

        protected override IList<ModificationBuilder<IEnumerable<LineNode>>> CreateModifications(
            ActivityExecutionContext context)
        {
            var model = context.GetVariable<object>("Model")!;
            var importContents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "Module_ImportSharedModule", model);
            var sharedModuleContents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "Module_SharedModule", model);

            int LineExpression(IEnumerable<LineNode> lines) =>
                lines.Last(l => l.IsMath($"{EntityInfo.NamespaceLastPart}RoutingModule")).LineNumber;

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

        public ModuleStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}