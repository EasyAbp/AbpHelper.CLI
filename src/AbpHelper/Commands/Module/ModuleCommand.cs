using System;
using EasyAbp.AbpHelper.Commands.Module.Install;
using JetBrains.Annotations;

namespace EasyAbp.AbpHelper.Commands.Module
{
    public class ModuleCommand : CommandBase
    {
        public ModuleCommand([NotNull] IServiceProvider serviceProvider) 
            : base(serviceProvider, "module", "Help quickly install/update/uninstall ABP modules. See 'abphelper module --help' for details")
        {
            AddCommand<InstallCommand>();
        }
    }
}