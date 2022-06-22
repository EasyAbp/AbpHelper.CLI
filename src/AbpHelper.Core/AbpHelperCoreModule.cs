using System.Reflection;
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

        private void ConfigureTemplateFiles(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IFileProvider>(sp => new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly()));
        }
    }
}