using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class DirectoryFinderStep : Step
    {
        public const string DefaultDirectoryParameterName = "DirectoryFinderResult";
        public string BaseDirectory { get; set; } = string.Empty;
        public string SearchDirectoryName { get; set; } = string.Empty;
        public string ResultParameterName { get; set; } = DefaultDirectoryParameterName;

        protected override Task RunStep()
        {
            var baseDirectory = BaseDirectory.IsNullOrEmpty() ? GetParameter<string>("BaseDirectory") : BaseDirectory;
            LogInput(() => baseDirectory);
            LogInput(() => SearchDirectoryName);

            var directoryPathName = Directory.EnumerateDirectories(baseDirectory, SearchDirectoryName, SearchOption.AllDirectories).Single();
            SetParameter(ResultParameterName, directoryPathName);
            LogOutput(() => directoryPathName, $"Found directory: {directoryPathName}, stored in parameter: [{ResultParameterName}]");

            return Task.CompletedTask;
        }
    }
}