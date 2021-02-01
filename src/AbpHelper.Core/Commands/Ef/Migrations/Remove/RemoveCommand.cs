using System;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Common;
using Elsa.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Commands.Ef.Migrations.Remove
{
    public class RemoveCommand : CommandWithOption<RemoveCommandOption>
    {
        private const string RemoveCommandDescription = @"Removes the last migration. The usage is the same as `dotnet ef migrations remove`, 
except providing the default value for the `--project` and `--startup-project` options.";

        public RemoveCommand(IServiceProvider serviceProvider) : base(serviceProvider, "remove", RemoveCommandDescription)
        {
        }

        protected override IActivityBuilder ConfigureBuild(RemoveCommandOption option, IActivityBuilder activityBuilder)
        {
            string efOptions = option.EfOptions == null ? String.Empty : string.Join(" ", option.EfOptions);
            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(step =>
                    {
                        step.VariableName = "EfOptions";
                        step.ValueExpression = new LiteralExpression(efOptions);

                    })
                    .AddConfigureMigrationProjectsWorkflow(ActivityNames.RemoveMigration)
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef migrations remove -p \"${MigrationProjectFile}\" -s \"${StartupProjectFile}\" ${EfOptions || ''}`")
                    ).WithName(ActivityNames.RemoveMigration)
                ;
        }
    }
}