using System;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow;
using EasyAbp.AbpHelper.Workflow.Common;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Commands.Ef.Migrations.Add
{
    public class AddCommand : CommandWithOption<AddCommandOption>
    {
        private const string AddCommandDescription = @"Add a new migration. The usage is the same as `dotnet ef migrations add`, 
except providing the default value for the `--project` and `--startup-project` options if not specified.";

        public AddCommand(IServiceProvider serviceProvider) : base(serviceProvider, "add", AddCommandDescription)
        {
        }

        protected override IActivityBuilder ConfigureBuild(AddCommandOption option, IActivityBuilder activityBuilder)
        {
            return base.ConfigureBuild(option, activityBuilder)
                    .AddConfigureMigrationProjectsWorkflow(ActivityNames.RunMigration)
                    .Then<RunCommandStep>(
                        step => step.Command = new JavaScriptExpression<string>("`dotnet ef migrations add ${Option.Name} -p \"${MigrationProjectFile}\" -s \"${StartupProjectFile}\"`")
                    ).WithName(ActivityNames.RunMigration)
                ;
        }
    }
}