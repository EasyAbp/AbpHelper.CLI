
using System;

namespace DosSEdo.AbpHelper.Commands
{
    public class GenerateCommand : CommandBase
    {
        public GenerateCommand(IServiceProvider serviceProvider) : base(serviceProvider, "generate", "Generate files for ABP projects. See 'abphelper generate --help' for details")
        {
            AddCommand<CrudCommand>();
            AddCommand<ServiceCommand>();
            AddCommand<MethodsCommand>();
            AddCommand<LocalizationCommand>();
        }
    }
}