using System.Collections.Generic;

namespace EasyAbp.AbpHelper.Core.Models
{
    public class GetInstalledPackagesOutput
    {
        public string SolutionName { get; set; }
        
        public Dictionary<string, List<PackageInfo>> Items { get; set; }
    }
}