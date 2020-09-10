using System;
using EasyAbp.AbpHelper.Commands.Generate.Controller;
using EasyAbp.AbpHelper.Commands.Generate.Crud;
using EasyAbp.AbpHelper.Commands.Generate.Localization;
using EasyAbp.AbpHelper.Commands.Generate.Methods;
using EasyAbp.AbpHelper.Commands.Generate.Service;
using Elsa.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Commands.Generate
{
    public class GenerateCommand<TOption> : CommandWithOption<TOption> where TOption : GenerateCommandOption
    {
        public GenerateCommand(IServiceProvider serviceProvider) : base(serviceProvider, "generate", "Generate files for ABP projects. See 'abphelper generate --help' for details")
        {
            AddAlias("gen");

            AddCommand<CrudCommand>();
            AddCommand<ServiceCommand>();
            AddCommand<MethodsCommand>();
            AddCommand<LocalizationCommand>();
            AddCommand<ControllerCommand>();
        }

        protected override IActivityBuilder ConfigureBuild(TOption option, IActivityBuilder activityBuilder)
        {
            return base.ConfigureBuild(option, activityBuilder)
                    .Then<SetVariable>(
                        step =>
                        {
                            step.VariableName = OverwriteVariableName;
                            step.ValueExpression = new JavaScriptExpression<bool>($"!{OptionVariableName}.{nameof(GenerateCommandOption.NoOverwrite)}");
                        })
                ;
        }
    }
}