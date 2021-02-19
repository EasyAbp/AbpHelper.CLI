using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Services;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Serilog;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.AbpHelper.Services
{
    public class CheckUpdateService : ICheckUpdateService, ITransientDependency
    {
        private const string RepoUrl = "https://api.nuget.org/v3/index.json";
        private const string PackageId = "EasyAbp.AbpHelper";

        public async Task CheckUpdateAsync()
        {
            Version latestVersion;
            try
            {
                CancellationToken cancellationToken = CancellationToken.None;
                SourceCacheContext cache = new SourceCacheContext();
                SourceRepository repository = Repository.Factory.GetCoreV3(RepoUrl);
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
            
                var versions = await resource.GetAllVersionsAsync(
                    PackageId,
                    cache,
                    NullLogger.Instance, 
                    cancellationToken);

                latestVersion = versions.Max(ver => ver.Version)!;
            }
            catch (Exception)
            {
                Log.Warning("Failed to get the latest version from nuget");
                return;
            }
            
            var currentVersion = Assembly.GetEntryAssembly()!.GetName().Version;

            if (currentVersion < latestVersion)
            {
                Log.Warning($"There is a new version of ABPHelper: {latestVersion.ToString(3)}");
                Log.Warning($"Use `dotnet tool update {PackageId} -g` to update");
            }
        }
    }
}