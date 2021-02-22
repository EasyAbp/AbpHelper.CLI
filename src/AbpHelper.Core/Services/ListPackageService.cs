using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using EasyAbp.AbpHelper.Core.Models;

namespace EasyAbp.AbpHelper.Core.Services
{
    public class ListPackageService : IListPackageService
    {
        public Task<Dictionary<string, List<PackageInfo>>> GetInstalledPackagesAsync(string baseDirectory)
        {
            var result = new Dictionary<string, List<PackageInfo>>();

            var csprojFiles = Directory.EnumerateFiles(baseDirectory, "*.csproj");
            foreach (var csprojFile in csprojFiles)
            {
                string projectName = Path.GetFileNameWithoutExtension(csprojFile);
                result.Add(projectName, new List<PackageInfo>());

                var root = XDocument.Load(csprojFile).Root;
                foreach (var element in root.Descendants("PackageReference"))
                {
                    string name = element.Attribute("Include")!.Value;
                    string version = element.Attribute("Version")!.Value;
                    result[projectName].Add(new PackageInfo(name, version));
                }
            }

            return Task.FromResult(result);
        }
    }
}