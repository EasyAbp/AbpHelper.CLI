using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AbpHelper.Steps
{
    public interface IStep : ITransientDependency
    {
        Task Run();
    }
}