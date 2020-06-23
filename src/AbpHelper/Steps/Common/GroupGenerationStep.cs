using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Extensions;
using DosSEdo.AbpHelper.Generator;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;

namespace DosSEdo.AbpHelper.Steps.Common
{
    public class GroupGenerationStep : Step
    {
        private readonly TextGenerator _textGenerator;
        private readonly IFileProvider _fileProvider;
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

        public GroupGenerationStep(TextGenerator textGenerator, IFileProvider fileProvider)
        {
            _textGenerator = textGenerator;
            _fileProvider = fileProvider;
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            string templateDir = await context.EvaluateAsync(TemplateDirectory, cancellationToken);
            LogInput(() => templateDir);
            LogInput(() => GroupName);
            string targetDirectory = await context.EvaluateAsync(TargetDirectory, cancellationToken);
            LogInput(() => targetDirectory);
            bool overwrite = await context.EvaluateAsync(Overwrite, cancellationToken);
            LogInput(() => Overwrite);
            object model = await context.EvaluateAsync(Model, cancellationToken);
            LogInput(() => model);

            string groupDir = Path.Combine(templateDir, "Groups", GroupName).NormalizePath();
            await GenerateFile(groupDir, targetDirectory, model, overwrite);

            return Done();
        }

        private async Task GenerateFile(string groupDirectory, string targetDirectory, object model, bool overwrite)
        {
            foreach ((string path, IFileInfo file) in _fileProvider.GetFilesRecursively(groupDirectory))
            {
                Logger.LogDebug($"Generating using template file: {path}");
                string targetFilePathNameTemplate = path.Replace(groupDirectory, targetDirectory);
                string targetFilePathName = _textGenerator.GenerateByTemplateText(targetFilePathNameTemplate, model);
                if (File.Exists(targetFilePathName) && !overwrite)
                {
                    Logger.LogInformation($"File {targetFilePathName} already exists, skip generating.");
                    continue;
                }

                string templateText = await file.ReadAsStringAsync();
                string contents = _textGenerator.GenerateByTemplateText(templateText, model, out TemplateContext context);

                context.CurrentGlobal.TryGetValue(SkipGenerate, out object value);
                if (value is bool skipGenerate && skipGenerate)
                {
                    Logger.LogInformation($"Evaluated value of `{SkipGenerate}` is true, skip generating.");
                    continue;
                }

                string? dir = Path.GetDirectoryName(targetFilePathName);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                await File.WriteAllTextAsync(targetFilePathName, contents);
                Logger.LogInformation($"File {targetFilePathName} successfully generated.");
            }
        }
    }
}