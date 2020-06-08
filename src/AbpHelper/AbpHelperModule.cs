using EasyAbp.AbpHelper.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace EasyAbp.AbpHelper
{
    [DependsOn(typeof(AbpAutofacModule))]
    [DependsOn(typeof(AbpVirtualFileSystemModule))]
    public class AbpHelperModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            ConfigureElsaActivities(context);
            ConfigureTemplateFiles(context);
        }

        private void ConfigureElsaActivities(ServiceConfigurationContext context)
        {
            context.Services
                .AddElsa()
                .AddAllActivities()
                ;
        }

        private void ConfigureTemplateFiles(ServiceConfigurationContext context)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<AbpHelperModule>("EasyAbp.AbpHelper");
            });
        }
    }
}