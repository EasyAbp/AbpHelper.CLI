using System;
using EasyAbp.AbpHelper.Commands.Generate.Controller;
using EasyAbp.AbpHelper.Commands.Generate.Crud;
using EasyAbp.AbpHelper.Commands.Generate.Localization;
using EasyAbp.AbpHelper.Commands.Generate.Methods;
using EasyAbp.AbpHelper.Commands.Generate.Service;

namespace EasyAbp.AbpHelper.Commands.Generate
{
    public class GenerateCommand : CommandBase
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
    }
}