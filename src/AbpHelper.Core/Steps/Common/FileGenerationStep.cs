using System.IO;
using System.Threading.Tasks;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    [Activity(
        Category = "FileGenerationStep",
        Description = "FileGenerationStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class FileGenerationStep : Step
    {
        [ActivityInput(
            Hint = "TargetFile",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string TargetFile
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "Contents",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string Contents
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            LogInput(() => TargetFile);
            LogInput(() => Contents, $"Contents length: {Contents.Length}");

            var dir = Path.GetDirectoryName(TargetFile)!;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Logger.LogInformation($"Directory {dir} created.");
            }

            await File.WriteAllTextAsync(TargetFile, Contents);
            Logger.LogInformation($"File {TargetFile} generated.");

            return Done();
        }
    }
}