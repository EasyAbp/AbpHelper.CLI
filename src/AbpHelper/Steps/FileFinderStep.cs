using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class FileFinderStep : IStep<FileFinderStepInput, FileFinderStepOutput>
    {
        public Task<FileFinderStepOutput> Run(FileFinderStepInput input)
        {
            var baseDirectory = input.BaseDirectory;
            var searchFileName = input.SearchFileName;

            var filePathName = Directory.EnumerateFiles(baseDirectory, searchFileName, SearchOption.AllDirectories).Single();
            return Task.FromResult(new FileFinderStepOutput(filePathName));
        }
    }

    public class FileFinderStepInput
    {
        public FileFinderStepInput(string baseDirectory, string searchFileName)
        {
            BaseDirectory = baseDirectory;
            SearchFileName = searchFileName;
        }

        public string BaseDirectory { get; }
        public string SearchFileName { get; }
    }

    public class FileFinderStepOutput
    {
        public FileFinderStepOutput(string filePathName)
        {
            FilePathName = filePathName;
        }

        public string FilePathName { get; }
    }
}