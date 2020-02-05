using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class FileFinderStep : Step
    {
        public string SearchFileName { get; set; } = string.Empty;
        public string ResultParameterName { get; set; } = "FilePathName";

        protected override Task RunStep()
        {
            var baseDirectory = GetParameter<string>("BaseDirectory");
            LogInput(() => baseDirectory);
            LogInput(() => SearchFileName);

            var filePathName = Directory.EnumerateFiles(baseDirectory, SearchFileName, SearchOption.AllDirectories).Single();
            SetParameter(ResultParameterName, filePathName);
            LogOutput(() => filePathName, $"Found file: {filePathName}, stored in parameter: [{ResultParameterName}]");

            return Task.CompletedTask;
        }
    }
}