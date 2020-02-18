using EasyAbp.AbpHelper.Steps.Common;
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
                ;
        }
    }
}