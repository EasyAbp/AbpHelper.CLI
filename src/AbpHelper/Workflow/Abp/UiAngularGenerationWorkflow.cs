using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.Typescript;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class UiAngularGenerationWorkflow
    {
        public static IActivityBuilder AddUiAngularGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    /* Add angular module */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>(
                            @"`cd /d ${BaseDirectory}\\angular && yarn ng generate module ${EntityInfo.NamespaceLastPart.toLowerCase()} --route ${EntityInfo.NamespaceLastPart.toLowerCase()} --module app.module`"
                        ))
                    /* Modify app-routing.module.ts */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("app-routing.module.ts")
                        )
                    .Then<AppRoutingModuleStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}