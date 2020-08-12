﻿using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using System;
using System.Collections.Generic;

namespace EasyAbp.AbpHelper.Commands
{
    public class LocalizationCommand : CommandWithOption<LocalizationCommandOption>
    {
        public LocalizationCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "localization", "Generate localization item(s) according to the specified name(s)")
        {
        }

        protected override IActivityBuilder ConfigureBuild(IActivityBuilder activityBuilder, LocalizationCommandOption option)
        {
            return base.ConfigureBuild(activityBuilder, option)
                .Then<SetVariable>(
                    step =>
                    {
                        step.VariableName = "TemplateDirectory";
                        step.ValueExpression = new LiteralExpression<string>("/Templates/Localization");
                    })
                .Then<ProjectInfoProviderStep>()
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