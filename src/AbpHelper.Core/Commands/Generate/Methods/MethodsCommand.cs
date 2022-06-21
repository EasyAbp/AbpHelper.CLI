using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Abp.ParseStep;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Generate;
using Elsa;
using Elsa.Builders;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using IActivityBuilder = Elsa.Builders.IActivityBuilder;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Methods
{
    public class MethodsCommand : CommandWithOption<MethodsCommandOption>
    {
        public MethodsCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "methods", "Generate service method(s) according to the specified name(s)")
        {
        }

        public override Task RunCommand(MethodsCommandOption option)
        {
            for (var i = 0; i < option.MethodNames.Length; i++)
            {
                // Convert method name to pascal case
                option.MethodNames[i] = option.MethodNames[i].ToPascalCase();
            }

            return base.RunCommand(option);
        }

        protected override IActivityBuilder ConfigureBuild(MethodsCommandOption option,
            IActivityBuilder activityBuilder)
        {
            return base.ConfigureBuild(option, activityBuilder)
                .AddOverwriteWorkflow(option)
                .Then<SetVariable>(step =>
                {
                    step.Set(x => x.VariableName, VariableNames.TemplateDirectory);
                    step.Set(x => x.Value, "/Templates/Methods");
                })
                .Then<FileFinderStep>(step =>
                {
                    step.Set(x => x.SearchFileName, x => $"I{option.ServiceName}AppService.cs");
                })
                .Then<InterfaceParserStep>()
                .Then<SetModelVariableStep>()
                .Then<AppServiceInterfaceStep>()
                .Then<FileModifierStep>()
                .Then<ForEach>(
                    step =>
                    {
                        step.Set(x => x.Items, option.MethodNames);
                    },
                    branch =>
                        branch.When(OutcomeNames.Iterate)
                            .Then<SetVariable>(step =>
                            {
                                step.Set(x => x.VariableName, "Bag.Name");
                                step.Set(x => x.Value, x => x.GetInput<string>());
                            })
                            .Then<SetModelVariableStep>()
                            .Then<GroupGenerationStep>(
                                step =>
                                {
                                    step.Set(x => x.GroupName, x => "Service");
                                    step.Set(x => x.TargetDirectory, x => x.GetVariable<string>(VariableNames.AspNetCoreDir));
                                }
                            )
                )
                .Then<FileFinderStep>(step =>
                {
                    step.Set(x => x.SearchFileName, x => $"{option.ServiceName}AppService.cs");
                })
                .Then<AppServiceClassStep>()
                .Then<FileModifierStep>();
        }
    }
}