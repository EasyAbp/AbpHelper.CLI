using System;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Common;
using Elsa.Activities.ControlFlow;
using Elsa.Builders;
using Elsa.Activities.Primitives;
using IActivityBuilder = Elsa.Builders.IActivityBuilder;

namespace EasyAbp.AbpHelper.Core.Commands.Ef.Migrations.Remove
{
    public class RemoveCommand : CommandWithOption<RemoveCommandOption>
    {
        private const string RemoveCommandDescription =
            @"Removes the last migration. The usage is the same as `dotnet ef migrations remove`, 
except providing the default value for the `--project` and `--startup-project` options.";

        public RemoveCommand(IServiceProvider serviceProvider) : base(serviceProvider, "remove",
            RemoveCommandDescription)
        {
        }

        protected override IActivityBuilder ConfigureBuild(RemoveCommandOption option, IActivityBuilder activityBuilder)
        {
            var efOptions = option.EfOptions == null ? string.Empty : string.Join(" ", option.EfOptions);
            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(step =>
                    {
                        step.Set(x => x.VariableName, "EfOptions");
                        step.Set(x => x.Value, efOptions);
                    })
                    .AddConfigureMigrationProjectsWorkflow(ActivityNames.RemoveMigration)
                    .Then<RunCommandStep>(
                        step =>
                        {
                            step.Set(x => x.Command,
                                "dotnet ef migrations remove -p \"{MigrationProjectFile}\" -s \"{StartupProjectFile}\" {EfOptions || ''}");
                        }).WithName(ActivityNames.RemoveMigration)
                ;
        }
    }
}