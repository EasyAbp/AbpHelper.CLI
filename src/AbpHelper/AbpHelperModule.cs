using AbpHelper.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AbpHelper
{
    [DependsOn(typeof(AbpAutofacModule))]
    public class AbpHelperModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services
                .AddElsa()
                .AddAllActivities()
                ;
        }
    }
}