﻿using System;
using EasyAbp.AbpHelper.Core.Commands.Module.Add;
using EasyAbp.AbpHelper.Core.Commands.Module.Remove;

namespace EasyAbp.AbpHelper.Core.Commands.Module
{
    public class ModuleCommand : CommandBase
    {
        public ModuleCommand(IServiceProvider serviceProvider) 
            : base(serviceProvider, "module", "Help quickly install/update/uninstall ABP modules. See 'abphelper module --help' for details")
        {
            AddCommand<AddCommand>();
            AddCommand<RemoveCommand>();
        }
    }
}