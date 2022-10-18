using System.IO;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using Volo.Abp.VirtualFileSystem;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    [Activity(
        Category = "GroupGenerationStep",
        Description = "GroupGenerationStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class GroupGenerationStep : Step
    {
        private readonly TextGenerator _textGenerator;
        private readonly IVirtualFileProvider _virtualFileProvider;
        private const string SkipGenerate = "SKIP_GENERATE";

        [ActivityInput(
            Hint = "TemplateDirectory",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? TemplateDirectory
        {
            get => GetState<string?>();
            set => SetState(value);
        }
        
        [ActivityInput(
            Hint = "GroupName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string GroupName
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "TargetDirectory",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? TargetDirectory
        {
            get => GetState<string?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "Overwrite",
            UIHint = ActivityInputUIHints.Checkbox,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public bool? Overwrite
        {
            get => GetState<bool?>();
            set => SetState(value);
        }

        public GroupGenerationStep(
            TextGenerator textGenerator,
            IVirtualFileProvider virtualFileProvider)
        {
            _textGenerator = textGenerator;
            _virtualFileProvider = virtualFileProvider;
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            TemplateDirectory ??= context.GetVariable<string>(VariableNames.TemplateDirectory)!;
            TargetDirectory ??= context.GetVariable<string>("BaseDirectory")!;
            Overwrite ??= context.GetVariable<bool>("Overwrite")!;

            LogInput(() => GroupName);
            LogInput(() => TemplateDirectory);
            LogInput(() => TargetDirectory);
            LogInput(() => Overwrite);

            var groupDir = Path.Combine(TemplateDirectory, "Groups", GroupName).NormalizePath();
            
            await GenerateFile(groupDir, TargetDirectory, context.GetVariable<object>("Model")!, Overwrite.Value);

            return Done();
        }

        private async Task GenerateFile(string groupDirectory, string targetDirectory, object model, bool overwrite)
        {
            foreach (var (path, file) in _virtualFileProvider.GetFilesRecursively(groupDirectory))
            {
                Logger.LogDebug($"Generating using template file: {path}");
                var targetFilePathNameTemplate = path.Replace(groupDirectory, targetDirectory);
                var targetFilePathName = _textGenerator.GenerateByTemplateText(targetFilePathNameTemplate, model);
                if (File.Exists(targetFilePathName) && !overwrite)
                {
                    Logger.LogInformation($"File {targetFilePathName} already exists, skip generating.");
                    continue;
                }

                var templateText = await file.ReadAsStringAsync();
                var contents = _textGenerator.GenerateByTemplateText(templateText, model, out TemplateContext context);

                context.CurrentGlobal.TryGetValue(SkipGenerate, out var value);
                if (value is true)
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