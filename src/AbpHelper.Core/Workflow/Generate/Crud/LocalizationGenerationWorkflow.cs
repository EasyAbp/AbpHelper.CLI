using System.Collections.Generic;
using EasyAbp.AbpHelper.Core.Commands;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class LocalizationGenerationWorkflow
    {
        public static IActivityBuilder AddLocalizationGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    /* Add localization */
                    .Then<TextGenerationStep>(
                        step => { step.Set(x => x.TemplateName, "Localization"); }
                    ).WithName(ActivityNames.LocalizationGeneration)
                    .Then<DirectoryFinderStep>(
                        step =>
                        {
                            step.Set(x => x.SearchDirectoryName, "Localization");
                            step.Set(x => x.BaseDirectory, x =>
                            {
                                var aspNetCoreDir = x.GetVariable<string>(VariableNames.AspNetCoreDir)!;
                                var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                return $"{aspNetCoreDir}/src/{projectInfo.FullName}.Domain.Shared";
                            });
                            step.Set(x => x.ExcludeDirectories,
                                x => x.GetVariable<string[]>(CommandConsts.ExcludeDirectoriesVariableName));
                        }
                    )
                    .Then<MultiFileFinderStep>(
                        step =>
                        {
                            step.Set(x => x.SearchFileName, "*.json");
                            step.Set(x => x.BaseDirectory,
                                x => x.GetVariable<string>(DirectoryFinderStep.DefaultDirectoryParameterName));
                            step.Set(x => x.ExcludeDirectories,
                                x => x.GetVariable<string[]>(CommandConsts.ExcludeDirectoriesVariableName));
                        }
                    )
                    .Then<ForEach>(
                        step =>
                        {
                            step.Set(x => x.Items,
                                x => x.GetVariable<IList<object>>(MultiFileFinderStep.DefaultFileParameterName));
                        },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<LocalizationJsonModificationCreatorStep>(
                                    step =>
                                    {
                                        step.Set(x => x.TargetFile, x => x.GetInput<string>());
                                        step.Set(x => x.LocalizationJson,
                                            x => x.GetVariable<string>(TextGenerationStep
                                                .DefaultGeneratedTextParameterName));
                                    }
                                )
                    )
                ;
        }
    }
}