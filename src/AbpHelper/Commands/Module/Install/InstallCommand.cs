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
            var projectNames = typeof(ModuleCommandOption).GetProperties()
                    .Where(prop => prop.PropertyType == typeof(bool) && (bool) prop.GetValue(option)!)
                    .Select(prop => _packageProjectMap[prop.Name.ToKebabCase()])
                    .ToArray()
                ;

            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(
                        step =>
                        {
                            step.VariableName = VariableNames.ProjectNames;
                            step.ValueExpression = new JavaScriptExpression<string[]>($"[{string.Join(",", projectNames.Select(n => $"'{n}'"))}]");
                        }
                    )
                    .Then<ForEach>(
                        x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(VariableNames.ProjectNames); },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<RunCommandStep>(
                                    step => step.Command = new JavaScriptExpression<string>(
                                        @"`cd /d ${AspNetCoreDir}/src/${ProjectInfo.FullName}.${CurrentValue} && dotnet add package ${Option.ModuleName}.${CurrentValue}`"
                                    )
                                )
                                .Then(branch)
                    )
                ;
        }
    }
}