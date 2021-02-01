using System;
using EasyAbp.AbpHelper.Core.Commands.Module.Add;
using EasyAbp.AbpHelper.Core.Commands.Module.Remove;
using JetBrains.Annotations;

namespace EasyAbp.AbpHelper.Core.Commands.Module
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