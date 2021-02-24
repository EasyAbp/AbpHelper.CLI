using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Models;

namespace EasyAbp.AbpHelper.Core.Services
{
    public interface IListPackageService
    {
        Task<GetInstalledPackagesOutput> GetInstalledPackagesAsync(string baseDirectory);
    }
}