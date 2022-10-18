using System;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Common;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Commands.Ef.Migrations.Add
{
    public class AddCommand : CommandWithOption<AddCommandOption>
    {
        private const string AddCommandDescription =
            @"Adds a new migration. The usage is the same as `dotnet ef migrations add`, 
except providing the default value for the `--project` and `--startup-project` options.";

        public AddCommand(IServiceProvider serviceProvider) : base(serviceProvider, "add", AddCommandDescription)
        {
        }

        protected override IActivityBuilder ConfigureBuild(AddCommandOption option, IActivityBuilder activityBuilder)
        {
            var efOptions = option.EfOptions == null ? string.Empty : string.Join(" ", option.EfOptions);
            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(step =>
                    {
                        step.Set(x => x.VariableName, "EfOptions");
                        step.Set(x => x.Value, efOptions);
                    })
                    .AddConfigureMigrationProjectsWorkflow(ActivityNames.AddMigration)
                    .Then<RunCommandStep>(
                        step =>
                        {
                            step.Set(x => x.Command,
                                "dotnet ef migrations add {Option.Name} -p \"{MigrationProjectFile}\" -s \"{StartupProjectFile}\" {EfOptions || ''}");
                        }).WithName(ActivityNames.AddMigration)
                ;
        }
    }
}