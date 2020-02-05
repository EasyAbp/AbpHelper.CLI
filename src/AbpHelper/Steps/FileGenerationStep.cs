using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AbpHelper.Steps
{
    public class FileGenerationStep : Step
    {
        public string File { get; set; } = string.Empty;
        public string Contents { get; set; } = string.Empty;

        protected override async Task RunStep()
        {
            var targetFile = File.IsNullOrEmpty() ? GetParameter<string>("FilePathName") : File;

            LogInput(() => targetFile);
            LogInput(() => Contents, $"Contents length: {Contents.Length}");

            var dir = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Logger.LogInformation($"Directory {dir} created.");
            }

            await System.IO.File.WriteAllTextAsync(targetFile, Contents);
            Logger.LogInformation($"File {targetFile} generated.");
        }
    }
}