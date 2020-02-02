using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Models;
using AbpHelper.Workflow;

namespace AbpHelper.Steps
{
    public class FileFinderStep : StepBase
    {
        public FileFinderStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public string SearchFileName { get; set; } = string.Empty;

        protected override Task RunStep()
        {
            var baseDirectory = GetParameter<ProjectInfo>("ProjectInfo").BaseDirectory;
            LogInput(() => baseDirectory);
            LogInput(() => SearchFileName);

            var filePathName = Directory.EnumerateFiles(baseDirectory, SearchFileName, SearchOption.AllDirectories).Single();
            SetParameter("FilePathName", filePathName);
            LogOutput(() => filePathName);

            return Task.CompletedTask;
        }
    }
}