using System;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Common
{
    public static class ConfigureHasDbMigrationsWorkflow
    {
        private const string SearchDbMigrationsResultName = nameof(SearchDbMigrationsResultName);

        public static IActivityBuilder AddConfigureHasDbMigrationsWorkflow(this IActivityBuilder builder,
            string nextActivityName)
        {
            return builder
                    .Then<FileFinderStep>(step =>
                    {
                        step.Set(x => x.SearchFileName, "*.EntityFrameworkCore.DbMigrations.csproj");
                        step.Set(x => x.ErrorIfNotFound, false);
                    })
                    .Then<If>(
                        step => step.Set(x => x.Condition, x => !x.GetInput<string>().IsNullOrWhiteSpace()),
                        ifElse =>
                        {
                            ifElse.When(OutcomeNames.True)
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.HasDbMigrations);
                                    step.Set(x => x.Value, true);
                                })
                                .ThenNamed(nextActivityName)
                                ;
                            ifElse.When(OutcomeNames.False)
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.HasDbMigrations);
                                    step.Set(x => x.Value, false);
                                })
                                .ThenNamed(nextActivityName)
                                ;
                        })
                ;
        }
    }
}