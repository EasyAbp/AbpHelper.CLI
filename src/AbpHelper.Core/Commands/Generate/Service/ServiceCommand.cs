using System;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Generate;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using Elsa.Builders;
using Humanizer;


namespace EasyAbp.AbpHelper.Core.Commands.Generate.Service
{
    public class ServiceCommand : CommandWithOption<ServiceCommandOption>
    {
        public ServiceCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "service",
                "Generate service interface and class files according to the specified name")
        {
        }

        public override Task RunCommand(ServiceCommandOption option)
        {
            if (option.Folder.IsNullOrEmpty())
            {
                option.Folder = option.Name.Pluralize();
            }

            option.Folder = option.Folder.NormalizePath();
            return base.RunCommand(option);
        }

        protected override IActivityBuilder ConfigureBuild(ServiceCommandOption option,
            IActivityBuilder activityBuilder)
        {
            return base.ConfigureBuild(option, activityBuilder)
                .AddOverwriteWorkflow(option)
                .Then<SetVariable>(step =>
                {
                    step.Set(x => x.VariableName, VariableNames.TemplateDirectory);
                    step.Set(x => x.Value, "/Templates/Service");
                })
                .Then<SetModelVariableStep>()
                .Then<GroupGenerationStep>(step =>
                {
                    step.Set(x => x.GroupName, "Service");
                    step.Set(x => x.TargetDirectory, x => x.GetVariable<string>(VariableNames.AspNetCoreDir));
                });
        }
    }
}