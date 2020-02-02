using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Workflow;

namespace AbpHelper.Steps
{
    public class FileFinderStep : Step
    {
        public FileFinderStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public string BaseDirectory { get; set; } = string.Empty;
        public string SearchFileName { get; set; } = string.Empty;

        protected override Task RunStep()
        {
            LogInput(() => BaseDirectory);
            LogInput(() => SearchFileName);

            var filePathName = Directory.EnumerateFiles(BaseDirectory, SearchFileName, SearchOption.AllDirectories).Single();
            SetParameter("FilePathName", filePathName);
            LogOutput(() => filePathName);

            return Task.CompletedTask;
        }
    }
}