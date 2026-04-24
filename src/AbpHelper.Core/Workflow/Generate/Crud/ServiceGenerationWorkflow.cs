using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.Console.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using System;
using System.Linq;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
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
                            step.TargetDirectory = new JavaScriptExpression<string>(VariableNames.AspNetCoreDir);
                        }
                    ).WithName(name)
                    /* Generate permissions */
                    .IfElse(
                        ifElse => ifElse.ConditionExpression = new JavaScriptExpression<bool>("Option.SkipPermissions"),
                        ifElse =>
                        {
                            ifElse
                                .When(OutcomeNames.True)
                                .Then(ActivityNames.AutoMapper)
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
                                .Then(ActivityNames.AutoMapper)
                                ;
                        }
                    )
                    /* Add mapping */
                    .Then<Switch>(@switch =>
                    {
                        @switch.Expression = new JavaScriptExpression<string>("ProjectInfo.MapperType");
                        @switch.Cases = Enum.GetValues(typeof(MapperType)).Cast<int>().Select(u => u.ToString()).ToArray();
                    },
                     @switch =>
                     {
                         @switch.When(MapperType.AutoMapper.ToString("D"))
                                .Then<FileFinderStep>(step => step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}ApplicationAutoMapperProfile.cs`"))
                                .Then<ApplicationAutoMapperProfileStep>()
                                .Then<FileModifierStep>()
                                .Then("AfterMappingStep");

                         @switch.When(MapperType.Mapperly.ToString("D"))
                                .Then<FileFinderStep>(step => step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}ApplicationMappers.cs`"))
                                .Then<ApplicationMapperlyStep>()
                                .Then<FileModifierStep>()
                                .Then("AfterMappingStep");

                     }).WithName(ActivityNames.AutoMapper)
                     .Then<NullStep>()
                     .WithName("AfterMappingStep")
                ;
        }
    }
}