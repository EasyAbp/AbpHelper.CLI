using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class FileFinderStep : Step
    {
        public string SearchFileName { get; set; } = string.Empty;

        protected override Task RunStep()
        {
            var baseDirectory = GetParameter<string>("BaseDirectory");
            LogInput(() => baseDirectory);
            LogInput(() => SearchFileName);

            var filePathName = Directory.EnumerateFiles(baseDirectory, SearchFileName, SearchOption.AllDirectories).Single();
            SetParameter("FilePathName", filePathName);
            LogOutput(() => filePathName);

            return Task.CompletedTask;
        }
    }
}