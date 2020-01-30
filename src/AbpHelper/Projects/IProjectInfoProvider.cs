using System.Threading.Tasks;
using AbpHelper.Dtos;
using Volo.Abp.DependencyInjection;

namespace AbpHelper.Projects
{
    public interface IProjectInfoProvider : ITransientDependency
    {
        Task<ProjectInfo> Get(string projectBaseDirectory);
        
    }
}