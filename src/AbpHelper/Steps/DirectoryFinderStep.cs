using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class DirectoryFinderStep : Step
    {
        public string SearchDirectoryName { get; set; } = string.Empty;
        public string ResultParameterName { get; set; } = "DirectoryPathName";

        protected override Task RunStep()
        {
            var baseDirectory = GetParameter<string>("BaseDirectory");
            LogInput(() => baseDirectory);
            LogInput(() => SearchDirectoryName);

            var directoryPathName = Directory.EnumerateDirectories(baseDirectory, SearchDirectoryName, SearchOption.AllDirectories).Single();
            SetParameter(ResultParameterName, directoryPathName);
            LogOutput(() => directoryPathName, $"Found directory: {directoryPathName}, stored in parameter: [{ResultParameterName}]");

            return Task.CompletedTask;
        }
    }
}