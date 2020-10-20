using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow;
using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using JetBrains.Annotations;

namespace EasyAbp.AbpHelper.Commands.Module.Add
{
    public class AddCommand : CommandWithOption<AddCommandOption>
    {
        private readonly IDictionary<string, string> _packageProjectMap = new Dictionary<string, string>
        {
            {ModuleConsts.Shared, "Domain.Shared"},
            {ModuleConsts.Domain, "Domain"},
            {ModuleConsts.EntityFrameworkCore, "EntityFrameworkCore"},
            {ModuleConsts.MongoDB, "MongoDB"},
            {ModuleConsts.Contracts, "Application.Contracts"},
            {ModuleConsts.Application, "Application"},
            {ModuleConsts.HttpApi, "HttpApi"},
            {ModuleConsts.Client, "HttpApi.Client"},
            {ModuleConsts.Web, "Web"},
        };

        public AddCommand([NotNull] IServiceProvider serviceProvider) : base(serviceProvider, "add", "Add ABP module according to the specified packages")
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
            var projectNames = typeof(ModuleCommandOption).GetProperties()
                    .Where(prop => prop.PropertyType == typeof(bool) && (bool) prop.GetValue(option)!)
                    .Select(prop => _packageProjectMap[prop.Name.ToKebabCase()])
                    .ToArray()
                ;

            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(
                        step =>
                        {
                            step.VariableName = VariableNames.TemplateDirectory;
                            step.ValueExpression = new LiteralExpression<string>("/Templates/Module");
                        })
                    .Then<SetVariable>(
                        step =>
                        {
                            step.VariableName = VariableNames.ProjectNames;
                            step.ValueExpression = new JavaScriptExpression<string[]>($"[{string.Join(",", projectNames.Select(n => $"'{n}'"))}]");
                        }
                    )
                    .Then<SetModelVariableStep>()
                    .Then<ForEach>(
                        x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(VariableNames.ProjectNames); },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = VariableNames.ModuleClassNamePostfix;
                                        step.ValueExpression = new JavaScriptExpression<string>("CurrentValue.replace('.', '')");
                                    }
                                )
                                .Then<SetVariable>(
                                    step =>
                                    {
                                        step.VariableName = VariableNames.DependsOnModuleClassName;
                                        step.ValueExpression = new JavaScriptExpression<string>($"{CommandConsts.OptionVariableName}.{nameof(ModuleCommandOption.ModuleNameLastPart)} + {VariableNames.ModuleClassNamePostfix} + 'Module'");
                                    }
                                )
                                .Then<IfElse>(
                                    step => step.ConditionExpression = new JavaScriptExpression<bool>($"{CommandConsts.OptionVariableName}.{nameof(AddCommandOption.Version)} != null"),
                                    ifElse =>
                                    {
                                        ifElse
                                            .When(OutcomeNames.True) // with version specified 
                                            .Then<RunCommandStep>(
                                                step => step.Command = new JavaScriptExpression<string>(
                                                    @"`cd /d ${AspNetCoreDir}/src/${ProjectInfo.FullName}.${CurrentValue} && dotnet add package ${Option.ModuleName}.${CurrentValue} -v ${Option.Version}`"
                                                ))
                                            .Then(ActivityNames.AddDependsOn)
                                            ;
                                        ifElse
                                            .When(OutcomeNames.False) // no version
                                            .Then<RunCommandStep>(
                                                step => step.Command = new JavaScriptExpression<string>(
                                                    @"`cd /d ${AspNetCoreDir}/src/${ProjectInfo.FullName}.${CurrentValue} && dotnet add package ${Option.ModuleName}.${CurrentValue}`"
                                                ))
                                            .Then(ActivityNames.AddDependsOn)
                                            ;
                                    }
                                )
                                .Then<EmptyStep>().WithName(ActivityNames.AddDependsOn)
                                .Then<FileFinderStep>(
                                    step => { step.SearchFileName = new JavaScriptExpression<string>($"`${{ProjectInfo.Name}}${{{VariableNames.ModuleClassNamePostfix}}}Module.cs`"); })
                                .Then<DependsOnStep>(step =>
                                {
                                    step.Action = new LiteralExpression<DependsOnStep.ActionType>(((int)DependsOnStep.ActionType.Add).ToString());
                                })
                                .Then<FileModifierStep>()
                                .Then<IfElse>(
                                    step => step.ConditionExpression = new JavaScriptExpression<bool>("CurrentValue == 'EntityFrameworkCore'"),
                                    ifElse =>
                                    {
                                        // For "EntityFrameCore" package, we generate a "builder.ConfigureXXX();" in the migrations context class */
                                        ifElse
                                            .When(OutcomeNames.True)
                                            .Then<FileFinderStep>(
                                                step => { step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}MigrationsDbContext.cs`"); }
                                            )
                                            .Then<MigrationsContextStep>(step =>
                                            {
                                                step.Action = new LiteralExpression<MigrationsContextStep.ActionType>(((int)MigrationsContextStep.ActionType.Add).ToString());
                                            })
                                            .Then<FileModifierStep>()
                                            .Then(ActivityNames.NextProject)
                                            ;
                                        ifElse
                                            .When(OutcomeNames.False)
                                            .Then(ActivityNames.NextProject)
                                            ;
                                    }
                                )
                                .Then<EmptyStep>().WithName(ActivityNames.NextProject)
                                .Then(branch)
                    )
                ;
        }
    }
}