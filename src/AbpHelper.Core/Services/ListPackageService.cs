using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using EasyAbp.AbpHelper.Core.Models;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.AbpHelper.Core.Services
{
    public class ListPackageService : IListPackageService, ITransientDependency
    {
        public Task<GetInstalledPackagesOutput> GetInstalledPackagesAsync(string baseDirectory)
        {
            var result = new Dictionary<string, List<PackageInfo>>();

            var slnFile = Directory.EnumerateFiles(baseDirectory, "*.sln", SearchOption.AllDirectories).First();
            
            var solutionName = Path.GetFileNameWithoutExtension(slnFile);

            var csprojFiles = Directory.EnumerateFiles(baseDirectory, "*.csproj", SearchOption.AllDirectories);
            
            foreach (var csprojFile in csprojFiles)
            {
                var projectName = Path.GetFileNameWithoutExtension(csprojFile);

                result.Add(projectName, new List<PackageInfo>());

                var root = XDocument.Load(csprojFile).Root;
                
                foreach (var element in root.Descendants("PackageReference"))
                {
                    var name = element.Attribute("Include")!.Value;
                    
                    var version = element.Attribute("Version")!.Value;
                    
                    result[projectName].Add(new PackageInfo(name, version));
                }
            }

            return Task.FromResult(new GetInstalledPackagesOutput
            {
                SolutionName = solutionName,
                Items = result
            });
        }
    }
}