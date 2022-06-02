using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Workflow.Common
{
    public static class ConfigureFindDbContextWorkflow
    {
        public static IActivityBuilder AddConfigureFindDbContextWorkflow(this IActivityBuilder builder, string nextActivityName)
        {
            return builder
                    .AddConfigureHasDbMigrationsWorkflow("FindDbContext")
                    .Then<IfElse>(
                        ifElse => ifElse.ConditionExpression = new JavaScriptExpression<bool>(VariableNames.HasDbMigrations),
                        ifElse =>
                        {
                            ifElse
                                .When(OutcomeNames.True)
                                .Then<FileFinderStep>(
                                    step => { step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}MigrationsDbContext.cs`"); }
                                )
                                .Then(nextActivityName)
                            ;
                            ifElse
                                .When(OutcomeNames.False)
                                .Then<FileFinderStep>(
                                    step => { step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContext.cs`"); }
                                )
                                .Then(nextActivityName)
                            ;
                        }
                    ).WithName("FindDbContext");
            ;
        }
    }
}
