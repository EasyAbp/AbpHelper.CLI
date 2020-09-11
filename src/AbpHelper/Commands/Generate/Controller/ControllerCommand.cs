using System;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Steps.Abp.ParseStep;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow;
using EasyAbp.AbpHelper.Workflow.Generate;
using EasyAbp.AbpHelper.Workflow.Generate.Crud;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Commands.Generate.Controller
{
    public class ControllerCommand : CommandWithOption<ControllerCommandOption>
    {
        public ControllerCommand(IServiceProvider serviceProvider) 
            : base(serviceProvider, "controller", "Generate controller class and methods according to the specified service")
        {
        }

        protected override IActivityBuilder ConfigureBuild(ControllerCommandOption option, IActivityBuilder activityBuilder)
        {
            return base.ConfigureBuild(option, activityBuilder)
                .AddOverwriteWorkflow()
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = VariableNames.TemplateDirectory;
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Controller");
                    })
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{nameof(ControllerCommandOption.SkipBuild)}"),
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.False)
                            .Then<RunCommandStep>(
                                step => step.Command = new JavaScriptExpression<string>(
                                    @"`cd /d ${AspNetCoreDir}/src/${ProjectInfo.FullName}.Application && dotnet build`"
                                ))
                            .Then(ActivityNames.SearchServiceInterface)
                            ;
                        ifElse.When(OutcomeNames.True)
                            .Then(ActivityNames.SearchServiceInterface)
                            ;
                    })
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new JavaScriptExpression<string>($"`I${{{OptionVariableName}.{nameof(ControllerCommandOption.Name)}}}AppService.cs`"); }
                ).WithName(ActivityNames.SearchServiceInterface)
                .Then<InterfaceParserStep>()
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new JavaScriptExpression<string>($"`${{{OptionVariableName}.{nameof(ControllerCommandOption.Name)}}}AppService.cs`"); }
                )
                .Then<ClassParserStep>()
                .Then<SetModelVariableStep>()
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{nameof(ControllerCommandOption.NoOverwrite)}"),
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.True) // Regenerate/Overwrite
                            .Then<GroupGenerationStep>(
                                step =>
                                {
                                    step.GroupName = "Controller";
                                    step.TargetDirectory = new JavaScriptExpression<string>(VariableNames.AspNetCoreDir);
                                })
                            ;
                        ifElse.When(OutcomeNames.False)
                            .Then<FileFinderStep>(
                                step =>
                                {
                                    step.SearchFileName = new JavaScriptExpression<string>($"`${{{OptionVariableName}.{nameof(ControllerCommandOption.Name)}}}Controller.cs`");
                                    step.ErrorIfNotFound = new JavaScriptExpression<bool>("false");
                                }
                            ).WithName(ActivityNames.SearchController)
                            .Then<IfElse>(
                                step => step.ConditionExpression = new JavaScriptExpression<bool>("FileFinderResult != null"),
                                found =>
                                {
                                    found.When(OutcomeNames.False)
                                        .Then<GroupGenerationStep>(
                                            step =>
                                            {
                                                step.GroupName = "Controller";
                                                step.TargetDirectory = new JavaScriptExpression<string>(VariableNames.AspNetCoreDir);
                                            })
                                        ;
                                    found.When(OutcomeNames.True)
                                        .Then<ClassParserStep>(step =>
                                        {
                                            step.OutputVariableName = new LiteralExpression<string>("ControllerInfo");
                                        })
                                        .Then<ControllerStep>()
                                        .Then<FileModifierStep>()
                                        ;
                                }
                            );
                    });
        }
    }
}