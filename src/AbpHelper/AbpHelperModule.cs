using System.Reflection;
using EasyAbp.AbpHelper.Core;
using EasyAbp.AbpHelper.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace EasyAbp.AbpHelper
{
    [DependsOn(typeof(AbpHelperCoreModule))]
    public class AbpHelperModule : AbpModule
    {
    }
}