using System.Collections.Generic;
using DosSEdo.AbpHelper.Steps.Abp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace DosSEdo.AbpHelper.Workflow.Generate.Crud
{
    public static class LocalizationGenerationWorkflow
    {
        public static IActivityBuilder AddLocalizationGenerationWorkflow(this IOutcomeBuilder builder, string name)
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
                            step.BaseDirectory = new JavaScriptExpression<string>(@"`${AspNetCoreDir}/src/${ProjectInfo.FullName}.Domain.Shared/Localization`");
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