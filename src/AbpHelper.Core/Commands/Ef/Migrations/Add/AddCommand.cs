using System;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Common;
using Elsa.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Commands.Ef.Migrations.Add
{
    public class AddCommand : CommandWithOption<AddCommandOption>
    {
        private const string AddCommandDescription = @"Adds a new migration. The usage is the same as `dotnet ef migrations add`, 
except providing the default value for the `--project` and `--startup-project` options.";

        public AddCommand(IServiceProvider serviceProvider) : base(serviceProvider, "add", AddCommandDescription)
        {
        }

        protected override IActivityBuilder ConfigureBuild(AddCommandOption option, IActivityBuilder activityBuilder)
        {
            string efOptions = option.EfOptions == null ? String.Empty : string.Join(" ", option.EfOptions);
            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(step =>
                    {
                        step.VariableName = "EfOptions";
                        step.ValueExpression = new LiteralExpression(efOptions);

                    })
                    .AddConfigureMigrationProjectsWorkflow(ActivityNames.AddMigration)
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef migrations add ${Option.Name} -p \"${MigrationProjectFile}\" -s \"${StartupProjectFile}\" ${EfOptions || ''}`")
                    ).WithName(ActivityNames.AddMigration)
                ;
        }
    }
}