using EasyAbp.AbpHelper;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace EasyApp.AbpHelper.Tests
{
    [DependsOn(
        typeof(AbpTestBaseModule),
        typeof(AbpHelperModule)
    )]
    public class AbpHelperTestModule : AbpModule
    {
    }
}