using EasyAbp.AbpHelper.Core;
using EasyAbp.AbpHelper.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace EasyAbp.AbpHelper
{
    [DependsOn(typeof(AbpHelperCoreModule))]
    public class AbpHelperModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services
                .AddElsa(x =>
                {
                    x.AddAbpHelperActivities();
                });
        }
    }
}