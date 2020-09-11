using System;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow;
using EasyAbp.AbpHelper.Workflow.Generate;
using Elsa.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using Humanizer;

namespace EasyAbp.AbpHelper.Commands.Generate.Service
{
    public class ServiceCommand : CommandWithOption<ServiceCommandOption>
    {
        public ServiceCommand(IServiceProvider serviceProvider) 
            : base(serviceProvider, "service", "Generate service interface and class files according to the specified name")
        {
        }

        protected override Task RunCommand(ServiceCommandOption option)
        {
            if (option.Folder.IsNullOrEmpty())
            {
                option.Folder = option.Name.Pluralize();
            }
            option.Folder = option.Folder.NormalizePath();
            return base.RunCommand(option);
        }

        protected override IActivityBuilder ConfigureBuild(ServiceCommandOption option, IActivityBuilder activityBuilder)
        {
            return base.ConfigureBuild(option, activityBuilder)
                .AddOverwriteWorkflow()
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = VariableNames.TemplateDirectory;
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Service");
                    })
                .Then<SetModelVariableStep>()
                .Then<GroupGenerationStep>(
                    step =>
                    {
                        step.GroupName = "Service";
                        step.TargetDirectory = new JavaScriptExpression<string>(VariableNames.AspNetCoreDir);
                    });
        }
    }
}