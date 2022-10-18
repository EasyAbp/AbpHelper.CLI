using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Common;
using Elsa;
using Elsa.Builders;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Primitives;
using IActivityBuilder = Elsa.Builders.IActivityBuilder;

namespace EasyAbp.AbpHelper.Core.Commands.Module.Add
{
    public class AddCommand : CommandWithOption<AddCommandOption>
    {
        private readonly IDictionary<string, string> _packageProjectMap = new Dictionary<string, string>
        {
            { ModuleConsts.Shared, "Domain.Shared" },
            { ModuleConsts.Domain, "Domain" },
            { ModuleConsts.EntityFrameworkCore, "EntityFrameworkCore" },
            { ModuleConsts.MongoDB, "MongoDB" },
            { ModuleConsts.Contracts, "Application.Contracts" },
            { ModuleConsts.Application, "Application" },
            { ModuleConsts.HttpApi, "HttpApi" },
            { ModuleConsts.Client, "HttpApi.Client" },
            { ModuleConsts.Web, "Web" },
        };

        public AddCommand(IServiceProvider serviceProvider) : base(serviceProvider, "add",
            "Add ABP module according to the specified packages")
        {
            AddValidator(result =>
            {
                if (!result.Children.Any(sr => sr.Symbol is Option opt && _packageProjectMap.Keys.Contains(opt.Name)))
                {
                    return "You must specify at least one package to add.";
                }

                return null;
            });
        }

        protected override IActivityBuilder ConfigureBuild(AddCommandOption option, IActivityBuilder activityBuilder)
        {
            var moduleIdToCustomsMapping = typeof(ModuleCommandOption).GetProperties()
                .Where(prop => prop.PropertyType == typeof(bool) && (bool)prop.GetValue(option)!)
                .Select(prop => _packageProjectMap[prop.Name.ToKebabCase()])
                .ToDictionary(x => x, x => new List<string>(new[] { $"{x}:{x}" }));

            if (!option.Custom.IsNullOrEmpty())
            {
                foreach (var customPart in option.Custom.Split(','))
                {
                    var moduleId = customPart.Substring(0, customPart.IndexOf(':'));

                    if (!moduleIdToCustomsMapping.ContainsKey(moduleId))
                    {
                        moduleIdToCustomsMapping.Add(moduleId, new List<string>());
                    }

                    moduleIdToCustomsMapping[moduleId].Add(customPart);
                }
            }

            var cdOption = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? " /d" : "";
            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(step =>
                    {
                        step.Set(x => x.VariableName, VariableNames.TemplateDirectory);
                        step.Set(x => x.Value, "/Templates/Module");
                    })
                    .Then<SetVariable>(step =>
                    {
                        step.Set(x => x.VariableName, VariableNames.ProjectNames);
                        step.Set(x => x.Value, moduleIdToCustomsMapping.ToDictionary(x => x.Value));
                    })
                    .Then<SetModelVariableStep>()
                    .Then<ForEach>(
                        step =>
                        {
                            step.Set(x => x.Items, x => x.GetVariable<IList<object>>(VariableNames.ProjectNames));
                        },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.CurrentModuleName);
                                    step.Set(x => x.Value, x => x.GetInput<string>()!.Split(':')[0]);
                                })
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.CurrentModuleName);
                                    step.Set(x => x.Value,
                                        x => x.GetInput<string>()!.Split(':')[1]);
                                })
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.SubmoduleUsingTextPostfix);
                                    step.Set(x => x.Value, x =>
                                    {
                                        var s = x.GetInput<string>()!.Split(':');
                                        return s.Length > 2 ? $".{s[2]}" : "";
                                    });
                                })
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.PackageName);
                                    step.Set(x => x.Value, x =>
                                    {
                                        var currentModuleName = x.GetVariable<string>(VariableNames.CurrentModuleName)!;
                                        return currentModuleName.IsNullOrWhiteSpace()
                                            ? option.ModuleName
                                            : $"{option.ModuleName}.{currentModuleName}";
                                    });
                                })
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.ModuleClassNamePostfix);
                                    step.Set(x => x.Value, x =>
                                    {
                                        var currentModuleName = x.GetVariable<string>(VariableNames.CurrentModuleName)!;
                                        return Regex.Replace(currentModuleName, "/\\./g", "");
                                    });
                                })
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.AppProjectClassNamePostfix);
                                    step.Set(x => x.Value, x =>
                                    {
                                        var currentModuleName = x.GetVariable<string>(VariableNames.CurrentModuleName)!;
                                        return Regex.Replace(currentModuleName, "/\\./g", "");
                                    });
                                })
                                .Then<SetVariable>(step =>
                                {
                                    step.Set(x => x.VariableName, VariableNames.DependsOnModuleClassName);
                                    step.Set(x => x.Value, x =>
                                    {
                                        var moduleClassNamePostfix =
                                            x.GetVariable<string>(VariableNames.ModuleClassNamePostfix);
                                        return
                                            $"{option.ModuleGroupNameWithoutCompanyName}{moduleClassNamePostfix}Module";
                                    });
                                })
                                .Then<If>(
                                    step => step.Set(x => x.Condition, !option.Version.IsNullOrWhiteSpace()),
                                    ifElse =>
                                    {
                                        ifElse
                                            .When(OutcomeNames.True) // with version specified 
                                            .Then<RunCommandStep>(step =>
                                            {
                                                step.Set(x => x.Command, x =>
                                                {
                                                    var aspNetCoreDir =
                                                        x.GetVariable<string>(VariableNames.AspNetCoreDir)!;
                                                    var targetAppProjectName =
                                                        x.GetVariable<string>(VariableNames.TargetAppProjectName)!;
                                                    var packageName =
                                                        x.GetVariable<string>(VariableNames.PackageName)!;
                                                    var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;

                                                    return
                                                        @$"cd{cdOption} {aspNetCoreDir}/src/{projectInfo.FullName}.{targetAppProjectName} && dotnet add package {packageName} -v {option.Version}";
                                                });
                                            })
                                            .ThenNamed(ActivityNames.AddDependsOn)
                                            ;
                                        ifElse
                                            .When(OutcomeNames.False) // no version
                                            .Then<RunCommandStep>(step =>
                                            {
                                                step.Set(x => x.Command, x =>
                                                {
                                                    var aspNetCoreDir =
                                                        x.GetVariable<string>(VariableNames.AspNetCoreDir)!;
                                                    var targetAppProjectName =
                                                        x.GetVariable<string>(VariableNames.TargetAppProjectName)!;
                                                    var packageName =
                                                        x.GetVariable<string>(VariableNames.PackageName)!;
                                                    var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;

                                                    return
                                                        $"cd{cdOption} {aspNetCoreDir}/src/{projectInfo.FullName}.{targetAppProjectName} && dotnet add package {packageName}";
                                                });
                                            })
                                            .ThenNamed(ActivityNames.AddDependsOn)
                                            ;
                                    }
                                )
                                .Then<EmptyStep>().WithName(ActivityNames.AddDependsOn)
                                .Then<FileFinderStep>(step =>
                                {
                                    step.Set(x => x.SearchFileName, x =>
                                    {
                                        var appProjectClassNamePostfix =
                                            x.GetVariable<string>(VariableNames.AppProjectClassNamePostfix)!;
                                        var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;

                                        return
                                            $"{projectInfo.Name}{appProjectClassNamePostfix}Module.cs";
                                    });
                                })
                                .Then<DependsOnStep>(step =>
                                {
                                    step.Set(x => x.Action, DependsOnStep.ActionType.Add);
                                    step.Set(x => x.ModuleClassNamePostfix,
                                        x => x.GetVariable<string>(VariableNames.ModuleClassNamePostfix));
                                    step.Set(x => x.DependsOnModuleClassName,
                                        x => x.GetVariable<string>(VariableNames.DependsOnModuleClassName));
                                    step.Set(x => x.SubmoduleUsingTextPostfix,
                                        x => x.GetVariable<string>(VariableNames.SubmoduleUsingTextPostfix));
                                })
                                .Then<FileModifierStep>()
                                .Then<If>(
                                    step =>
                                    {
                                        step.Set(x => x.Condition, x =>
                                        {
                                            var targetAppProjectName =
                                                x.GetVariable<string>(VariableNames.TargetAppProjectName)!;
                                            return targetAppProjectName == "EntityFrameworkCore";
                                        });
                                    },
                                    ifElse =>
                                    {
                                        // For "EntityFrameCore" package, we generate a "builder.ConfigureXXX();" in the migrations context class */
                                        ifElse
                                            .When(OutcomeNames.True)
                                            .Then<EmptyStep>()
                                            .AddConfigureFindDbContextWorkflow("AddAction")
                                            .Then<MigrationsContextStep>(step =>
                                            {
                                                step.Set(x => x.Action, MigrationsContextStep.ActionType.Add);
                                            }).WithName("AddAction")
                                            .Then<FileModifierStep>()
                                            .ThenNamed(ActivityNames.NextProject)
                                            ;
                                        ifElse
                                            .When(OutcomeNames.False)
                                            .ThenNamed(ActivityNames.NextProject)
                                            ;
                                    }
                                )
                                .Then<EmptyStep>().WithName(ActivityNames.NextProject)
                    )
                ;
        }
    }
}