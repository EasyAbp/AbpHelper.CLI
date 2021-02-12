using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.AbpHelper.Core.Services
{
    [Dependency(TryRegister = true)]
    public class NullCheckUpdateService : ICheckUpdateService, ITransientDependency
    {
        public Task CheckUpdateAsync()
        {
            return Task.CompletedTask;
        }
    }
}