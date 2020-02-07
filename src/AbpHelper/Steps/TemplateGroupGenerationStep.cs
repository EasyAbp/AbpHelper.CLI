using System;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Generator;
using Microsoft.Extensions.Logging;

namespace AbpHelper.Steps
{
    public class TemplateGroupGenerationStep : Step
    {
        public string GroupName { get; set; } = string.Empty;
        public string TargetDirectory { get; set; } = string.Empty;
        public bool Overwrite { get; set; }
        public object Model { get; set; } = new object();

        protected override async Task RunStep()
        {
            LogInput(() => GroupName);
            LogInput(() => TargetDirectory);
            if (ContainsParameter("Overwrite")) Overwrite = GetParameter<bool>("Overwrite");
            LogInput(() => Overwrite);
            LogInput(() => Model);

            var appDir = AppDomain.CurrentDomain.BaseDirectory!;
            var groupDir = Path.Combine(appDir, "Templates", "Groups", GroupName);
            if (!Directory.Exists(groupDir)) throw new DirectoryNotFoundException($"Template group directory {groupDir} is not exist.");

            await GenerateFile(groupDir, TargetDirectory, Model);
        }

        private async Task GenerateFile(string groupDirectory, string targetDirectory, object model)
        {
            foreach (var file in Directory.EnumerateFiles(groupDirectory, "*.sbntxt", SearchOption.AllDirectories))
            {
                Logger.LogDebug($"Generating using template file: {file}");
                var targetFilePathNameTemplate = file.Replace(groupDirectory, targetDirectory);
                var targetFilePathName = TextGenerator.GenerateByTemplateText(targetFilePathNameTemplate, model).RemovePostFix(".sbntxt");
                if (File.Exists(targetFilePathName) && !Overwrite)
                {
                    Logger.LogInformation($"File {targetFilePathName} already exist, skip generating.");
                    continue;
                }

                var dir = Path.GetDirectoryName(targetFilePathName);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var templateText = await File.ReadAllTextAsync(file);
                var contents = TextGenerator.GenerateByTemplateText(templateText, model);
                await File.WriteAllTextAsync(targetFilePathName, contents);
                Logger.LogInformation($"File {targetFilePathName} successfully generated.");
            }
        }
    }
}