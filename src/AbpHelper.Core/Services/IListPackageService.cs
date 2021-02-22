using System.Collections.Generic;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Models;

namespace EasyAbp.AbpHelper.Core.Services
{
    public interface IListPackageService
    {
        Task<Dictionary<string, List<PackageInfo>>> GetInstalledPackagesAsync(string baseDirectory);
    }
}