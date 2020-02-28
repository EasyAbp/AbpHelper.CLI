using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Generator;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;

namespace EasyAbp.AbpHelper.Steps.Common
{
    public class GroupGenerationStep : Step
    {
        private const string SkipGenerate = "SKIP_GENERATE";

        public WorkflowExpression<string> TemplateDirectory
        {
            get => GetState<WorkflowExpression<string>>(() => new JavaScriptExpression<string>("TemplateDirectory"));
            set => SetState(value);
        }
        
        public string GroupName
        {
            get => GetState<string>();
            set => SetState(value);
        }

        public WorkflowExpression<string> TargetDirectory
        {
            get => GetState(() => new JavaScriptExpression<string>("BaseDirectory"));
            set => SetState(value);
        }

        public WorkflowExpression<bool> Overwrite
        {
            get => GetState(() => new JavaScriptExpression<bool>("Overwrite"));
            set => SetState(value);
        }

        public WorkflowExpression<object> Model
        {
            get => GetState<WorkflowExpression<object>>(() => new JavaScriptExpression<object>("Model"));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            string templateDir = await context.EvaluateAsync(TemplateDirectory, cancellationToken);
            LogInput(() => templateDir);
            LogInput(() => GroupName);
            var targetDirectory = await context.EvaluateAsync(TargetDirectory, cancellationToken);
            LogInput(() => targetDirectory);
            var overwrite = await context.EvaluateAsync(Overwrite, cancellationToken);
            LogInput(() => Overwrite);
            var model = await context.EvaluateAsync(Model, cancellationToken);
            LogInput(() => model);

            var appDir = AppDomain.CurrentDomain.BaseDirectory!;
            var groupDir = Path.Combine(appDir, templateDir, "Groups", GroupName);
            if (!Directory.Exists(groupDir)) throw new DirectoryNotFoundException($"Template group directory {groupDir} is not exist.");

            await GenerateFile(groupDir, targetDirectory, model, overwrite);

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

                var templateText = await File.ReadAllTextAsync(file);
                var contents = TextGenerator.GenerateByTemplateText(templateText, model, out TemplateContext context);

                context.CurrentGlobal.TryGetValue(SkipGenerate, out object value);
                if (value is bool skipGenerate && skipGenerate)
                {
                    Logger.LogInformation($"Evaluated value of `{SkipGenerate}` is true, skip generating.");
                    continue;
                }

                var dir = Path.GetDirectoryName(targetFilePathName);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                await File.WriteAllTextAsync(targetFilePathName, contents);
                Logger.LogInformation($"File {targetFilePathName} successfully generated.");
            }
        }
    }
}