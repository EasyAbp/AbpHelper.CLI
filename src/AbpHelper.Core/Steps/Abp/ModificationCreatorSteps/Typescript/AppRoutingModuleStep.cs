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
        Category = "AppRoutingModuleStep",
        Description = "AppRoutingModuleStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class AppRoutingModuleStep : TypeScriptModificationCreatorStep
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
            var importContents = TextGenerator.GenerateByTemplateName(TemplateDirectory,
                "AppRoutingModule_ImportApplicationLayoutComponent", model);
            var routeContents =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "AppRoutingModule_Routing", model);

            int LineExpression(IEnumerable<LineNode> lines) =>
                lines.Last(l => l.IsMath($"{EntityInfo.NamespaceLastPart.ToLower()}")).LineNumber;

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
                    routeContents
                )
            };
        }

        public AppRoutingModuleStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}