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
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

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
                .AddOverwriteWorkflow()
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = VariableNames.TemplateDirectory;
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Methods");
                    })
                .Then<FileFinderStep>(
                    step =>
                    {
                        step.SearchFileName = new JavaScriptExpression<string>($"`I${{{OptionVariableName}.{nameof(MethodsCommandOption.ServiceName)}}}AppService.cs`");
                    })
                .Then<InterfaceParserStep>()
                .Then<SetModelVariableStep>()
                .Then<AppServiceInterfaceStep>()
                .Then<FileModifierStep>()
                .Then<ForEach>(
                    x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>($"{OptionVariableName}.{nameof(MethodsCommandOption.MethodNames)}"); },
                    branch =>
                        branch.When(OutcomeNames.Iterate)
                            .Then<SetVariable>(
                                step =>
                                {
                                    step.VariableName = "Bag.Name";
                                    step.ValueExpression = new JavaScriptExpression<string>("CurrentValue");
                                }
                            )
                            .Then<SetModelVariableStep>()
                            .Then<GroupGenerationStep>(
                                step =>
                                {
                                    step.GroupName = "Service";
                                    step.TargetDirectory = new JavaScriptExpression<string>(VariableNames.AspNetCoreDir);
                                }
                            )
                            .Then(branch)
                )
                .Then<FileFinderStep>(
                    step =>
                    {
                        step.SearchFileName = new JavaScriptExpression<string>($"`${{{OptionVariableName}.{nameof(MethodsCommandOption.ServiceName)}}}AppService.cs`");
                    })
                .Then<AppServiceClassStep>()
                .Then<FileModifierStep>();
        }
    }
}