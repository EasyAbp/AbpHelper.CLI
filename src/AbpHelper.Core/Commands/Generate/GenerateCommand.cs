using System;
using EasyAbp.AbpHelper.Core.Commands.Generate.Controller;
using EasyAbp.AbpHelper.Core.Commands.Generate.Crud;
using EasyAbp.AbpHelper.Core.Commands.Generate.Localization;
using EasyAbp.AbpHelper.Core.Commands.Generate.Methods;
using EasyAbp.AbpHelper.Core.Commands.Generate.Service;

namespace EasyAbp.AbpHelper.Core.Commands.Generate
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