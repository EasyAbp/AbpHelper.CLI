using System;
using Elsa.Services;
using JetBrains.Annotations;

namespace EasyAbp.AbpHelper.Commands.Module.Install
{
    public class InstallCommand : CommandWithOption<InstallCommandOption>
    {
        public InstallCommand([NotNull] IServiceProvider serviceProvider) : base(serviceProvider, "install", "Install ABP module according to the specified packages")
        {
            AddValidator(result =>
            {
                if (result.Children.Count <= 1)
                {
                    return "You must specify at least one package to install.";
                }

                return null;
            });
        }

        protected override IActivityBuilder ConfigureBuild(InstallCommandOption option, IActivityBuilder activityBuilder)
        {
            if (!(option.Shared || option.Domain || option.EntityFrameworkCore || option.MongoDB || option.Contract ||
                  option.Application || option.HttpApi || option.Client || option.Web))
            {
                
            }
            return base.ConfigureBuild(option, activityBuilder)
                
                ;
        }
    }
}