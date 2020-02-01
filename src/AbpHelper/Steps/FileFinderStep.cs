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

        protected override Task RunStep()
        {
            var baseDirectory = GetParameter<ProjectInfo>("ProjectInfo").BaseDirectory;
            var searchFileName = GetParameter<string>("SearchFileName");
            LogInput(() => baseDirectory);
            LogInput(() => searchFileName);

            var filePathName = Directory.EnumerateFiles(baseDirectory, searchFileName, SearchOption.AllDirectories).Single();
            SetParameter("FilePathName", filePathName);
            LogOutput(() => filePathName);

            return Task.CompletedTask;
        }
    }
}