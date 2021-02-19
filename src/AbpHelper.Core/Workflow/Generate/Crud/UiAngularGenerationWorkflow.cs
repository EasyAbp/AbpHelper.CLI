using System.Runtime.InteropServices;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.Typescript;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class UiAngularGenerationWorkflow
    {
        public static IActivityBuilder AddUiAngularGenerationWorkflow(this IOutcomeBuilder builder)
        {
            string cdOption = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? " /d" : "";
            return builder
                    /* Add angular module */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>(
                            @$"`cd{cdOption} ${{BaseDirectory}}/angular && yarn ng generate module ${{EntityInfo.NamespaceLastPart.toLowerCase()}} --route ${{EntityInfo.NamespaceLastPart.toLowerCase()}} --module app.module`"
                        ))
                    /* Modify app-routing.module.ts */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("app-routing.module.ts")
                    )
                    .Then<AppRoutingModuleStep>()
                    .Then<FileModifierStep>(
                        step => step.NewLine = new JavaScriptExpression<string>(@"'\n'")
                    )
                    /* Add list component */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>(
                            @$"`cd{cdOption} ${{BaseDirectory}}/angular && yarn ng generate component ${{EntityInfo.NamespaceLastPart.toLowerCase()}}/${{EntityInfo.Name.toLowerCase()}}-list`"
                        ))
                    /* Modify XXX.module.ts */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new JavaScriptExpression<string>("`${EntityInfo.NamespaceLastPart.toLowerCase()}.module.ts`")
                    )
                    .Then<ModuleStep>()
                    .Then<FileModifierStep>(
                        step => step.NewLine = new JavaScriptExpression<string>(@"'\n'")
                    )
                    /* Modify XXX-routing.module.ts */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new JavaScriptExpression<string>("`${EntityInfo.NamespaceLastPart.toLowerCase()}-routing.module.ts`")
                    )
                    .Then<RoutingModuleStep>()
                    .Then<FileModifierStep>(
                        step => step.NewLine = new JavaScriptExpression<string>(@"'\n'")
                    )
                    /* Create state */
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>(
                            @$"`cd{cdOption} ${{BaseDirectory}}/angular && yarn ng generate ngxs-schematic:state ${{EntityInfo.NamespaceLastPart.toLowerCase()}}`"
                        ))
                    /* Generate XXX.ts */
                    .Then<GroupGenerationStep>(
                        step => { step.GroupName = "UiAngular"; }
                    )
                ;
        }
    }
}