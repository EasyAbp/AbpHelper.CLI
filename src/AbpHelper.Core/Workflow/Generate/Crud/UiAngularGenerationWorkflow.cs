using System.Runtime.InteropServices;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.Typescript;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class UiAngularGenerationWorkflow
    {
        public static IActivityBuilder AddUiAngularGenerationWorkflow(this IOutcomeBuilder builder)
        {
            string cdOption = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? " /d" : "";
            return builder
                    /* Add angular module */
                    .Then<RunCommandStep>(step =>
                    {
                        step.Set(x => x.Command, x =>
                        {
                            var baseDirectory = x.GetVariable<string>("BaseDirectory");
                            var entityInfo = x.GetVariable<EntityInfo>("EntityInfo")!;
                            return
                                $"cd{cdOption} {baseDirectory}/angular && yarn ng generate module {entityInfo.NamespaceLastPart.ToLower()} --route {entityInfo.NamespaceLastPart.ToLower()} --module app.module";
                        });
                    })
                    /* Modify app-routing.module.ts */
                    .Then<FileFinderStep>(step => step.Set(x => x.SearchFileName, "app-routing.module.ts"))
                    .Then<AppRoutingModuleStep>(step =>
                    {
                        step.Set(x => x.EntityInfo, x => x.GetVariable<EntityInfo>("EntityInfo"));
                    })
                    .Then<FileModifierStep>(step => step.Set(x => x.NewLine, "\n"))
                    /* Add list component */
                    .Then<RunCommandStep>(step =>
                    {
                        step.Set(x => x.Command, x =>
                        {
                            var baseDirectory = x.GetVariable<string>("BaseDirectory");
                            var entityInfo = x.GetVariable<EntityInfo>("EntityInfo")!;
                            return
                                $"cd{cdOption} {baseDirectory}/angular && yarn ng generate component {entityInfo.NamespaceLastPart.ToLower()}/{entityInfo.Name.ToLower()}-list";
                        });
                    })
                    .Then<RunCommandStep>(step =>
                    {
                        step.Set(x => x.Command, x =>
                        {
                            var baseDirectory = x.GetVariable<string>("BaseDirectory");
                            var entityInfo = x.GetVariable<EntityInfo>("EntityInfo")!;
                            return
                                $"cd{cdOption} {baseDirectory}/angular && yarn ng generate component {entityInfo.NamespaceLastPart.ToLower()}/{entityInfo.Name.ToLower()}-list";
                        });
                    })
                    /* Modify XXX.module.ts */
                    .Then<FileFinderStep>(step => step.Set(x => x.SearchFileName, x =>
                    {
                        var entityInfo = x.GetVariable<EntityInfo>("EntityInfo")!;
                        return $"{entityInfo.NamespaceLastPart.ToLower()}.module.ts";
                    }))
                    .Then<ModuleStep>(step => step.Set(x => x.EntityInfo, x => x.GetVariable<EntityInfo>("EntityInfo")))
                    .Then<FileModifierStep>(step => step.Set(x => x.NewLine, "\n"))
                    /* Modify XXX-routing.module.ts */
                    .Then<FileFinderStep>(step => step.Set(x => x.SearchFileName, x =>
                    {
                        var entityInfo = x.GetVariable<EntityInfo>("EntityInfo")!;
                        return $"{entityInfo.NamespaceLastPart.ToLower()}-routing.module.ts";
                    }))
                    .Then<RoutingModuleStep>()
                    .Then<FileModifierStep>(step => step.Set(x => x.NewLine, "\n"))
                    /* Create state */
                    .Then<RunCommandStep>(step =>
                    {
                        step.Set(x => x.Command, x =>
                        {
                            var baseDirectory = x.GetVariable<string>("BaseDirectory");
                            var entityInfo = x.GetVariable<EntityInfo>("EntityInfo")!;
                            return
                                $"cd{cdOption} {baseDirectory}/angular && yarn ng generate ngxs-schematic:state {entityInfo.NamespaceLastPart.ToLower()}";
                        });
                    })
                    /* Generate XXX.ts */
                    .Then<GroupGenerationStep>(step => { step.Set(x => x.GroupName, "UiAngular"); }
                    )
                ;
        }
    }
}