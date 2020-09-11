using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using JetBrains.Annotations;

namespace EasyAbp.AbpHelper.Commands.Module.Install
{
    public class InstallCommand : CommandWithOption<InstallCommandOption>
    {
        private readonly IDictionary<string, string> _packageProjectMap = new Dictionary<string, string>
        {
            {ModuleConsts.Shared, "Domain.Shared"},
            {ModuleConsts.Domain, "Domain"},
            {ModuleConsts.EntityFrameworkCore, "EntityFrameworkCore"},
            {ModuleConsts.MongoDB, "MongoDB"},
            {ModuleConsts.Contract, "Application.Contracts"},
            {ModuleConsts.Application, "Application"},
            {ModuleConsts.HttpApi, "HttpApi"},
            {ModuleConsts.Client, "HttpApi.Client"},
            {ModuleConsts.Web, "Web"},
        };

        public InstallCommand([NotNull] IServiceProvider serviceProvider) : base(serviceProvider, "install", "Install ABP module according to the specified packages")
        {
            AddValidator(result =>
            {
                if (!result.Children.Any(sr => sr.Symbol is Option opt && _packageProjectMap.Keys.Contains(opt.Name)))
                {
                    return "You must specify at least one package to install.";
                }

                return null;
            });
        }

        protected override IActivityBuilder ConfigureBuild(InstallCommandOption option, IActivityBuilder activityBuilder)
        {
            var packageNames = typeof(ModuleCommandOption).GetProperties()
                    .Where(prop => prop.PropertyType == typeof(bool) && (bool) prop.GetValue(option)!)
                    .Select(prop => prop.Name)
                    .ToArray()
                ;
            var projectNames = packageNames.Select(name => _packageProjectMap[name])
                    .ToArray()
                ;
            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(
                        step =>
                        {
                            step.VariableName = VariableNames.PackageNames;
                            step.ValueExpression = new JavaScriptExpression<string[]>($"[{string.Join(",", packageNames)}]");
                        }
                    )
                    .Then<ForEach>(
                        x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(VariableNames.PackageNames); },
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
                    )
                    .Then<SetVariable>(
                        step =>
                        {
                            step.VariableName = VariableNames.ProjectNames;
                            step.ValueExpression = new JavaScriptExpression<string[]>($"[{string.Join(",", projectNames)}]");
                        }
                    )
                ;
        }
    }
}