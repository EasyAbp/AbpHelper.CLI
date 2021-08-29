using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class EfCoreConfigurationWorkflow
    {
        public static IActivityBuilder AddEfCoreConfigurationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    /* Add entity property to DbContext class*/
                    .Then<FileFinderStep>(
                        step => { step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContext.cs`"); })
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
                    /* Add entity configuration to ModelCreating */
                    .Then<FileFinderStep>(
                        step => { 
                            step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContextModelCreatingExtensions.cs`");
                            step.ErrorIfNotFound = new JavaScriptExpression<bool>("false");
                        }).WithName(ActivityNames.DbContextModel)
                    .IfElse(
                        step => step.ConditionExpression = new JavaScriptExpression<bool>("FileFinderResult != null"),
                        ifElse =>
                        {
                            /* For app using abp v4.4 before version and all versions of the module, using DbContextModelCreatingExtensions by default */
                            ifElse
                                .When(OutcomeNames.True)
                                .Then("DbContextModelCreating")
                                ;
                            /* For app using abp v4.4 after version, using ProjectNameDbContext by default */
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(
                                    step => step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContext.cs`")
                                )
                                .Then("DbContextModelCreating")
                                ;
                        }
                    )
                    .Then<DbContextModelCreatingExtensionsStep>().WithName("DbContextModelCreating")
                    .Then<FileModifierStep>()
                ;
        }
    }
}