using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class FileFinderStep : Step
    {
        public const string DefaultFilesParameterName = "FileFinderResult";
        public string BaseDirectory { get; set; } = string.Empty;
        public string SearchFileName { get; set; } = string.Empty;
        public string ResultParameterName { get; set; } = DefaultFilesParameterName;
        public bool Multiple { get; set; } = false;

        protected override Task RunStep()
        {
            var baseDirectory = BaseDirectory.IsNullOrEmpty() ? GetParameter<string>("BaseDirectory") : BaseDirectory;
            LogInput(() => baseDirectory);
            LogInput(() => SearchFileName);
            LogInput(() => Multiple);

            var files = Directory.EnumerateFiles(baseDirectory, SearchFileName, SearchOption.AllDirectories).ToArray();

            if (Multiple)
            {
                if (files.Length == 0) throw new FileNotFoundException();

                SetParameter(ResultParameterName, files);
                LogOutput(() => files, $"Found files count: {files.Length}, stored in parameter: [{ResultParameterName}]");
            }
            else
            {
                var filePathName = files.SingleOrDefault();
                if (filePathName == null) throw new FileNotFoundException();
                SetParameter(ResultParameterName, filePathName);
                LogOutput(() => filePathName, $"Found file: {filePathName}, stored in parameter: [{ResultParameterName}]");
            }

            return Task.CompletedTask;
        }
    }
}