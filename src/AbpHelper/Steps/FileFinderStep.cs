using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Models;

namespace AbpHelper.Steps
{
    public class FileFinderStep : StepBase
    {
        public FileFinderStep(WorkflowContext context) : base(context)
        {
        }

        public override Task Run()
        {
            var baseDirectory = GetParameter<ProjectInfo>("ProjectInfo").BaseDirectory;
            var searchFileName = GetParameter<string>("SearchFileName");

            var filePathName = Directory.EnumerateFiles(baseDirectory, searchFileName, SearchOption.AllDirectories).Single();
            SetParameter("FilePathName", filePathName);

            return Task.CompletedTask;
        }
    }
}