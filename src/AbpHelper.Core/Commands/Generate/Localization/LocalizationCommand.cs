using System;
using System.Collections.Generic;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Generate;
using Elsa;
using Elsa.Builders;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using Fluid.Ast;
using IActivityBuilder = Elsa.Builders.IActivityBuilder;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Localization
{
    public class LocalizationCommand : CommandWithOption<LocalizationCommandOption>
    {
        public LocalizationCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "localization", "Generate localization item(s) according to the specified name(s)")
        {
        }

        protected override IActivityBuilder ConfigureBuild(LocalizationCommandOption option,
            IActivityBuilder activityBuilder)
        {
            return base.ConfigureBuild(option, activityBuilder)
                .AddOverwriteWorkflow(option)
                .Then<SetVariable>(step =>
                {
                    step.Set(x => x.VariableName, VariableNames.TemplateDirectory);
                    step.Set(x => x.Value, "/Templates/Localization");
                })
                .Then<SetModelVariableStep>()
                /* Add localization */
                .Then<TextGenerationStep>(step => { step.Set(x => x.TemplateName, "Localization"); })
                .Then<DirectoryFinderStep>(step =>
                {
                    step.Set(x => x.SearchDirectoryName, "Localization");
                    step.Set(x => x.BaseDirectory, x =>
                    {
                        var aspNetCoreDir = x.GetVariable<string>(VariableNames.AspNetCoreDir);
                        var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                        return $"{aspNetCoreDir}/src/{projectInfo.FullName}.Domain.Shared";
                    });
                    step.Set(x => x.ExcludeDirectories, x => x.GetVariable<string[]>(ExcludeDirectoriesVariableName));
                })
                .Then<MultiFileFinderStep>(step =>
                {
                    step.Set(x => x.SearchFileName, "*.json");
                    step.Set(x => x.BaseDirectory,
                        x => x.GetVariable<string>(DirectoryFinderStep.DefaultDirectoryParameterName));
                    step.Set(x => x.ExcludeDirectories,
                        x => x.GetVariable<string[]>(CommandConsts.ExcludeDirectoriesVariableName));
                })
                .Then<ForEach>(
                    step =>
                    {
                        step.Set(x => x.Items,
                            x => x.GetVariable<IList<object>>(MultiFileFinderStep.DefaultFileParameterName));
                    },
                    branch =>
                        branch.When(OutcomeNames.Iterate)
                            .Then<LocalizationJsonModificationCreatorStep>(step =>
                            {
                                step.Set(x => x.TargetFile, x => x.GetInput<string>());
                                step.Set(x => x.LocalizationJson,
                                    x => x.GetVariable<string>(TextGenerationStep.DefaultGeneratedTextParameterName));
                            })
                );
        }
    }
}