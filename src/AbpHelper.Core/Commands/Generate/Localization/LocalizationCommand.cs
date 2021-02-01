using System;
using System.Collections.Generic;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Generate;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Localization
{
    public class LocalizationCommand : CommandWithOption<LocalizationCommandOption>
    {
        public LocalizationCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "localization", "Generate localization item(s) according to the specified name(s)")
        {
        }

        protected override IActivityBuilder ConfigureBuild(LocalizationCommandOption option, IActivityBuilder activityBuilder)
        {
            return base.ConfigureBuild(option, activityBuilder)
                .AddOverwriteWorkflow()
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = VariableNames.TemplateDirectory;
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Localization");
                    })
                .Then<SetModelVariableStep>()
                /* Add localization */
                .Then<TextGenerationStep>(
                    step => { step.TemplateName = "Localization"; }
                )
                .Then<MultiFileFinderStep>(
                    step =>
                    {
                        step.SearchFileName = new LiteralExpression("*.json");
                        step.BaseDirectory = new JavaScriptExpression<string>(@"`${AspNetCoreDir}/src/${ProjectInfo.FullName}.Domain.Shared/Localization`");
                    }
                )
                .Then<ForEach>(
                    x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(MultiFileFinderStep.DefaultFileParameterName); },
                    branch =>
                        branch.When(OutcomeNames.Iterate)
                            .Then<LocalizationJsonModificationCreatorStep>(
                                step =>
                                {
                                    step.TargetFile = new JavaScriptExpression<string>("CurrentValue");
                                    step.LocalizationJson = new JavaScriptExpression<string>(TextGenerationStep.DefaultGeneratedTextParameterName);
                                }
                            )
                            .Then(branch)
                );
        }
    }
}