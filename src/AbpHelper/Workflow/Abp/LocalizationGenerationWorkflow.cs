using System.Collections.Generic;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class LocalizationGenerationWorkflow
    {
        public static IActivityBuilder AddLocalizationGenerationWorkflow(this IActivityBuilder builder, string name)
        {
            return builder
                    /* Add localization */
                    .Then<TextGenerationStep>(
                        step => { step.TemplateName = "Localization"; }
                    ).WithName("LocalizationGeneration")
                    .Then<MultiFileFinderStep>(
                        step =>
                        {
                            step.SearchFileName = new LiteralExpression("*.json");
                            step.BaseDirectory = new JavaScriptExpression<string>(@"`${BaseDirectory}\\src\\${ProjectInfo.FullName}.Domain.Shared\\Localization`");
                        }
                    )
                    .Then<ForEach>(
                        x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(MultiFileFinderStep.DefaultFileParameterName); },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<LocalizationJsonModificationCreatorStep>(
                                    step =>
                                    {
                                        step.TargetFile = new JavaScriptExpression<string>("CurrentValue");
                                        step.LocalizationJson = new JavaScriptExpression<string>(TextGenerationStep.DefaultGeneratedTextParameterName);
                                    }
                                )
                                .Then(branch)
                    )
                ;
        }
    }
}