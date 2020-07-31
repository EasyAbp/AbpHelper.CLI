using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using System;

namespace EasyAbp.AbpHelper.Commands
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
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "TemplateDirectory";
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Controller");
                    })
                .Then<ProjectInfoProviderStep>()
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.SkipBuild"),
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.False)
                            .Then<RunCommandStep>(
                                step => step.Command = new JavaScriptExpression<string>(
                                    @"`cd /d ${AspNetCoreDir} && dotnet build`"
                                ))
                            .Then("SearchServiceInterface")
                            ;
                        ifElse.When(OutcomeNames.True)
                            .Then("SearchServiceInterface")
                            ;
                    })
                .Then<FileFinderStep>(
                    step => { step.SearchFileName = new JavaScriptExpression<string>($"`I${{{OptionVariableName}.Name}}AppService.cs`"); }
                ).WithName("SearchServiceInterface")
                .Then<ServiceInterfaceSemanticParserStep>()
                .Then<SetModelVariableStep>()
                .Then<IfElse>(
                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{OptionVariableName}.{OverwriteVariableName}"),
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.True) // Regenerate/Overwrite
                            .Then<GroupGenerationStep>(
                                step =>
                                {
                                    step.GroupName = "Controller";
                                    step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                                })
                            ;
                        ifElse.When(OutcomeNames.False)
                            .Then<FileFinderStep>(
                                step =>
                                {
                                    step.SearchFileName = new JavaScriptExpression<string>($"`${{{OptionVariableName}.Name}}Controller.cs`");
                                    step.ErrorIfNotFound = new JavaScriptExpression<bool>("false");
                                }
                            ).WithName("SearchController")
                            .Then<IfElse>(
                                step => step.ConditionExpression = new JavaScriptExpression<bool>("FileFinderResult == null"),
                                found =>
                                {
                                    found.When(OutcomeNames.True)
                                        .Then<GroupGenerationStep>(
                                            step =>
                                            {
                                                step.GroupName = "Controller";
                                                step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                                            })
                                        ;
                                    found.When(OutcomeNames.False)
                                        .Then<ControllerParserStep>()
                                        .Then<ControllerStep>()
                                        .Then<FileModifierStep>()
                                        ;
                                }
                            );
                    });
        }
    }
}