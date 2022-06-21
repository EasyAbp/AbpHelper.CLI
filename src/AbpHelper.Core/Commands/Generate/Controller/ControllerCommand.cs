using System;
using System.Runtime.InteropServices;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Abp.ParseStep;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Generate;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using Elsa.Builders;
using IActivityBuilder = Elsa.Builders.IActivityBuilder;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Controller
{
    public class ControllerCommand : CommandWithOption<ControllerCommandOption>
    {
        public ControllerCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "controller",
                "Generate controller class and methods according to the specified service")
        {
        }

        protected override IActivityBuilder ConfigureBuild(ControllerCommandOption option,
            IActivityBuilder activityBuilder)
        {
            string cdOption = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? " /d" : "";
            return base.ConfigureBuild(option, activityBuilder)
                .AddOverwriteWorkflow(option)
                .Then<SetVariable>(step =>
                {
                    step.Set(x => x.VariableName, VariableNames.TemplateDirectory);
                    step.Set(x => x.Value, "/Templates/Controller");
                })
                .Then<If>(
                    step => { step.Set(x => x.Condition, option.SkipBuild); },
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.False)
                            .Then<RunCommandStep>(
                                step =>
                                {
                                    step.Set(x => x.Command, x =>
                                    {
                                        var aspNetCoreDir = x.GetVariable<string>(VariableNames.AspNetCoreDir);
                                        var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                        return
                                            $"cd{cdOption} {aspNetCoreDir}/src/{projectInfo.FullName}.Application && dotnet build";
                                    });
                                })
                            .ThenNamed(ActivityNames.SearchServiceInterface)
                            ;
                        ifElse.When(OutcomeNames.True)
                            .ThenNamed(ActivityNames.SearchServiceInterface)
                            ;
                    })
                .Then<FileFinderStep>(step => { step.Set(x => x.SearchFileName, $"I{option.Name}AppService.cs"); }
                ).WithName(ActivityNames.SearchServiceInterface)
                .Then<InterfaceParserStep>()
                .Then<FileFinderStep>(step => { step.Set(x => x.SearchFileName, $"{option.Name}AppService.cs"); }
                )
                .Then<ClassParserStep>()
                .Then<SetModelVariableStep>()
                .Then<If>(
                    step => { step.Set(x => x.Condition, option.NoOverwrite); },
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.True) // Regenerate/Overwrite
                            .Then<GroupGenerationStep>(step =>
                            {
                                step.Set(x => x.GroupName, "Controller");
                                step.Set(x => x.TargetDirectory,
                                    x => x.GetVariable<string>(VariableNames.AspNetCoreDir));
                            })
                            ;
                        ifElse.When(OutcomeNames.False)
                            .Then<FileFinderStep>(step =>
                            {
                                step.Set(x => x.SearchFileName, $"{option.Name}Controller.cs");
                                step.Set(x => x.ErrorIfNotFound, false);
                            }).WithName(ActivityNames.SearchController)
                            .Then<If>(
                                step => { step.Set(x => x.Condition, x => !x.GetInput<string>().IsNullOrWhiteSpace()); },
                                found =>
                                {
                                    found.When(OutcomeNames.False)
                                        .Then<GroupGenerationStep>(step =>
                                        {
                                            step.Set(x => x.GroupName, "Controller");
                                            step.Set(x => x.TargetDirectory,
                                                x => x.GetVariable<string>(VariableNames.AspNetCoreDir));
                                        })
                                        ;
                                    found.When(OutcomeNames.True)
                                        .Then<ClassParserStep>(step =>
                                        {
                                            step.Set(x => x.OutputVariableName, "ControllerInfo");
                                        })
                                        .Then<ControllerStep>(step =>
                                        {
                                            step.Set(x => x.InterfaceInfo,
                                                x => x.GetVariable<TypeInfo>("InterfaceInfo"));
                                            step.Set(x => x.ClassInfo, x => x.GetVariable<TypeInfo>("ClassInfo"));
                                            step.Set(x => x.ControllerInfo,
                                                x => x.GetVariable<TypeInfo>("ControllerInfo"));
                                        })
                                        .Then<FileModifierStep>()
                                        ;
                                }
                            );
                    });
        }
    }
}