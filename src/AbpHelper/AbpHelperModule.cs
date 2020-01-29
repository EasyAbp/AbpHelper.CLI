using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AbpHelper
{
    [DependsOn(typeof(AbpAutofacModule))] 
    public class AbpHelperModule : AbpModule
    {
        
    }
}