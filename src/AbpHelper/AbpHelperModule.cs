using System.Collections.Generic;
using AbpHelper.Workflow;
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
            context.Services.AddScoped(sp => new WorkflowContext(new Dictionary<string, object>()));
        }
    }
}