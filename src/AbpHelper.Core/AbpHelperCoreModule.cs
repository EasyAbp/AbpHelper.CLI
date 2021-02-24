using System.Reflection;
using EasyAbp.AbpHelper.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace EasyAbp.AbpHelper.Core
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AbpVirtualFileSystemModule))]
    public class AbpHelperCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            ConfigureElsaActivities(context);
            ConfigureTemplateFiles(context);
            ConfigureVirtualFileSystem();
        }

        private void ConfigureVirtualFileSystem()
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<AbpHelperCoreModule>();
            });
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
            context.Services.AddSingleton<IFileProvider>(sp => new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly()));
        }
    }
}