using System.Reflection;
using DosSEdo.AbpHelper.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace DosSEdo.AbpHelper
{
    [DependsOn(typeof(AbpAutofacModule))]
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
            context.Services.AddSingleton<IFileProvider>(sp => new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly()));
        }
    }
}