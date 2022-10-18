using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Common
{
    public static class ConfigureFindDbContextWorkflow
    {
        public static IActivityBuilder AddConfigureFindDbContextWorkflow(this IActivityBuilder builder,
            string nextActivityName)
        {
            return builder
                .AddConfigureHasDbMigrationsWorkflow("FindDbContext")
                .Then<If>(
                    ifElse => ifElse.Set(x => x.Condition, x => x.GetVariable<bool>(VariableNames.HasDbMigrations)),
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.True)
                            .Then<FileFinderStep>(step =>
                            {
                                step.Set(x => x.SearchFileName, x =>
                                {
                                    var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                    return $"{projectInfo.Name}MigrationsDbContext.cs";
                                });
                            })
                            .ThenNamed(nextActivityName)
                            ;
                        ifElse
                            .When(OutcomeNames.False)
                            .Then<FileFinderStep>(step =>
                            {
                                step.Set(x => x.SearchFileName, x =>
                                {
                                    var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                    return $"{projectInfo.Name}DbContext.cs";
                                });
                            })
                            .ThenNamed(nextActivityName)
                            ;
                    }
                ).WithName("FindDbContext");
            ;
        }
    }
}