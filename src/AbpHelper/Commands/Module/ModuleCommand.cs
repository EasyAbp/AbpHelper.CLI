using System;
using EasyAbp.AbpHelper.Commands.Module.Add;
using EasyAbp.AbpHelper.Commands.Module.Remove;
using JetBrains.Annotations;

namespace EasyAbp.AbpHelper.Commands.Module
{
    public class ModuleCommand : CommandBase
    {
        public ModuleCommand([NotNull] IServiceProvider serviceProvider) 
            : base(serviceProvider, "module", "Help quickly install/update/uninstall ABP modules. See 'abphelper module --help' for details")
        {
            AddCommand<AddCommand>();
            AddCommand<RemoveCommand>();
        }
    }
}