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
using JetBrains.Annotations;
using IActivityBuilder = Elsa.Builders.IActivityBuilder;

namespace EasyAbp.AbpHelper.Core.Commands.Module.Remove
{
    public class RemoveCommand : CommandWithOption<RemoveCommandOption>
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

        public RemoveCommand([NotNull] IServiceProvider serviceProvider) : base(serviceProvider, "remove",
            "Remove ABP module according to the specified packages")
        {
            AddValidator(result =>
            {
                if (!result.Children.Any(sr => sr.Symbol is Option opt && _packageProjectMap.Keys.Contains(opt.Name)))
                {
                    return "You must specify at least one package to remove.";
                }

                return null;
            });
        }

        protected override IActivityBuilder ConfigureBuild(RemoveCommandOption option, IActivityBuilder activityBuilder)
        {
            var moduleNameToAppProjectNameMapping = typeof(ModuleCommandOption).GetProperties()
                .Where(prop => prop.PropertyType == typeof(bool) && (bool)prop.GetValue(option)!)
                .Select(prop => _packageProjectMap[prop.Name.ToKebabCase()])
                .ToDictionary(x => x, x => x);

            if (!option.Custom.IsNullOrEmpty())
            {
                foreach (var customPart in option.Custom.Split(','))
                {
                    var s = customPart.Split(":", 2);
                    moduleNameToAppProjectNameMapping.Add(s[0], s[1]);
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
                        step.Set(x => x.Value, moduleNameToAppProjectNameMapping);
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
                                    step.Set(x => x.VariableName, VariableNames.TargetAppProjectName);
                                    step.Set(x => x.Value, x => x.GetInput<string>()!.Split(':')[1]);
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
                                .Then<FileFinderStep>(step =>
                                {
                                    step.Set(x => x.SearchFileName, x =>
                                    {
                                        var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                        var postfix = x.GetVariable<string>(VariableNames.AppProjectClassNamePostfix);
                                        return $"{projectInfo.Name}{postfix}Module.cs";
                                    });
                                })
                                .Then<DependsOnStep>(step =>
                                {
                                    step.Set(x => x.Action, DependsOnStep.ActionType.Remove);
                                    step.Set(x => x.ModuleClassNamePostfix,
                                        x => x.GetVariable<string>(VariableNames.ModuleClassNamePostfix));
                                    step.Set(x => x.DependsOnModuleClassName,
                                        x => x.GetVariable<string>(VariableNames.DependsOnModuleClassName));
                                    step.Set(x => x.SubmoduleUsingTextPostfix,
                                        x => x.GetVariable<string>(VariableNames.SubmoduleUsingTextPostfix));
                                })
                                .Then<FileModifierStep>()
                                .Then<If>(
                                    step => step.Set(x => x.Condition,
                                        x =>
                                        {
                                            return x.GetVariable<string>(VariableNames.TargetAppProjectName) ==
                                                   "EntityFrameworkCore";
                                        }),
                                    ifElse =>
                                    {
                                        // For "EntityFrameCore" package, we generate a "builder.ConfigureXXX();" in the migrations context class */
                                        ifElse
                                            .When(OutcomeNames.True)
                                            .Then<EmptyStep>()
                                            .AddConfigureFindDbContextWorkflow("RemoveAction")
                                            .Then<MigrationsContextStep>(step =>
                                            {
                                                step.Set(x => x.Action, MigrationsContextStep.ActionType.Remove);
                                            }).WithName("RemoveAction")
                                            .Then<FileModifierStep>()
                                            .ThenNamed(ActivityNames.RemoveDependsOn)
                                            ;
                                        ifElse
                                            .When(OutcomeNames.False)
                                            .ThenNamed(ActivityNames.RemoveDependsOn)
                                            ;
                                    }
                                )
                                .Then<EmptyStep>().WithName(ActivityNames.RemoveDependsOn)
                                .Then<RunCommandStep>(step =>
                                {
                                    step.Set(x => x.Command, x =>
                                    {
                                        var aspNetCoreDir = x.GetVariable<string>(VariableNames.AspNetCoreDir);
                                        var projectInfo = x.GetVariable<ProjectInfo>("ProjectInfo")!;
                                        var targetAppProjectName =
                                            x.GetVariable<string>(VariableNames.TargetAppProjectName)!;
                                        var packageName = x.GetVariable<string>(VariableNames.PackageName)!;
                                        return
                                            $"cd{cdOption} {aspNetCoreDir}/src/{projectInfo.FullName}.{targetAppProjectName} && dotnet remove package {packageName}";
                                    });
                                })
                    )
                ;
        }
    }
}