using DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace DosSEdo.AbpHelper.Workflow.Generate.Crud
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
                                .Then("DbContextModel")
                                ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then("DbContextModel")
                                ;
                        }
                    )
                    /* Add entity configuration to DbContextModelCreatingExtensions */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("*DbContextModelCreatingExtensions.cs")
                    ).WithName("DbContextModel")
                    .Then<DbContextModelCreatingExtensionsStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}