using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using Humanizer;

namespace EasyAbp.AbpHelper.Models
{
    public class DtoInfo
    {
        public DtoInfo(string readTypeName, string createTypeName, string updateTypeName)
        {
            ReadTypeName = readTypeName;
            CreateTypeName = createTypeName;
            UpdateTypeName = updateTypeName;
        }

        public string ReadTypeName { get; }
        public string CreateTypeName { get; }
        public string UpdateTypeName { get; }
    }
}