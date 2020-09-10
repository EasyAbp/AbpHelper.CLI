using System;
using JetBrains.Annotations;

namespace EasyAbp.AbpHelper.Commands.Module.Install
{
    public class InstallCommand : CommandWithOption<InstallCommandOption>
    {
        public InstallCommand([NotNull] IServiceProvider serviceProvider) : base(serviceProvider, "install", "Install ABP module according to the specified packages")
        {
        }
    }
}