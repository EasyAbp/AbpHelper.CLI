using DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace DosSEdo.AbpHelper.Workflow.Generate.Crud
{
    public static class ServiceGenerationWorkflow
    {
        public static IActivityBuilder AddServiceGenerationWorkflow(this IActivityBuilder builder, string name)
        {
            return builder
                    /* Generate dto, service interface and class files */
                    .Then<GroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "Service";
                            step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                        }
                    ).WithName(name)
                    /* Generate permissions */
                    .IfElse(
                        ifElse => ifElse.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipPermissions"),
                        ifElse =>
                        {
                            ifElse
                                .When(OutcomeNames.True)
                                .Then("AutoMapper")
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(
                                    step => { step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}Permissions.cs`"); })
                                .Then<PermissionsStep>()
                                .Then<FileModifierStep>()
                                .Then<FileFinderStep>(
                                    step => { step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}PermissionDefinitionProvider.cs`"); })
                                .Then<PermissionDefinitionProviderStep>()
                                .Then<FileModifierStep>()
                                .Then("AutoMapper")
                                ;
                        }
                    )
                    /* Add mapping */
                    .Then<FileFinderStep>(step => step.SearchFileName = new LiteralExpression("*ApplicationAutoMapperProfile.cs")).WithName("AutoMapper")
                    .Then<ApplicationAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}