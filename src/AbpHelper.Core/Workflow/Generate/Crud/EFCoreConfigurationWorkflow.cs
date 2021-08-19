﻿using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class EfCoreConfigurationWorkflow
    {
        public static IActivityBuilder AddEfCoreConfigurationWorkflow(this IActivityBuilder builder, string name)
        {
            return builder
                    /* Add entity property to DbContext class*/
                    .Then<FileFinderStep>(
                        step => { step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContext.cs`"); }).WithName(name)
                    .Then<DbContextClassStep>()
                    .Then<FileModifierStep>()
                    .IfElse(
                        step => step.ConditionExpression = new JavaScriptExpression<bool>("ProjectInfo.TemplateType == 1"),
                        ifElse =>
                        {
                            // For module, we also need to modify the IDbContext interface */
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<FileFinderStep>(
                                    step => { step.SearchFileName = new JavaScriptExpression<string>("`I${ProjectInfo.Name}DbContext.cs`"); })
                                .Then<DbContextInterfaceStep>()
                                .Then<FileModifierStep>()
                                .Then(ActivityNames.DbContextModel)
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then(ActivityNames.DbContextModel)
                                ;
                        }
                    )
                    /* Add entity configuration to DbContextModelCreatingExtensions */
                    .IfElse(
                        step => step.ConditionExpression = new JavaScriptExpression<bool>(VariableNames.HasDbMigrations),
                        ifElse =>
                        {
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<FileFinderStep>(
                                    step => step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContextModelCreatingExtensions.cs`")
                                )
                                .Then("DbContextModelCreating")
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(
                                    step => step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContext.cs`")
                                )
                                .Then("DbContextModelCreating")
                                ;
                        }
                    ).WithName(ActivityNames.DbContextModel)
                    .Then<DbContextModelCreatingExtensionsStep>().WithName("DbContextModelCreating")
                    .Then<FileModifierStep>()
                ;
        }
    }
}