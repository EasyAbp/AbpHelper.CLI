using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AbpHelper.Generator;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;

namespace AbpHelper.Steps
{
    public class TemplateGroupGenerationStep : Step
    {
        public string GroupName
        {
            get => GetState<string>();
            set => SetState(value);
        }

        public string TargetDirectory
        {
            get => GetState<string>();
            set => SetState(value);
        }

        public WorkflowExpression<bool> Overwrite
        {
            get => GetState(() => new JavaScriptExpression<bool>("Overwrite"));
            set => SetState(value);
        }

        public object Model
        {
            get => GetState<object>();
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            LogInput(() => GroupName);
            LogInput(() => TargetDirectory);
            var overwrite = await context.EvaluateAsync(Overwrite, cancellationToken);
            LogInput(() => Overwrite);
            LogInput(() => Model);

            var appDir = AppDomain.CurrentDomain.BaseDirectory!;
            var groupDir = Path.Combine(appDir, "Templates", "Groups", GroupName);
            if (!Directory.Exists(groupDir)) throw new DirectoryNotFoundException($"Template group directory {groupDir} is not exist.");

            await GenerateFile(groupDir, TargetDirectory, Model, overwrite);

            return Done();
        }

        private async Task GenerateFile(string groupDirectory, string targetDirectory, object model, bool overwrite)
        {
            foreach (var file in Directory.EnumerateFiles(groupDirectory, "*.sbntxt", SearchOption.AllDirectories))
            {
                Logger.LogDebug($"Generating using template file: {file}");
                var targetFilePathNameTemplate = file.Replace(groupDirectory, targetDirectory);
                var targetFilePathName = TextGenerator.GenerateByTemplateText(targetFilePathNameTemplate, model).RemovePostFix(".sbntxt");
                if (File.Exists(targetFilePathName) && !overwrite)
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