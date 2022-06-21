using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    [Activity(
        Category = "MultiFileFinderStep",
        Description = "MultiFileFinderStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class MultiFileFinderStep : StepWithOption
    {
        public const string DefaultFileParameterName = "MultiFilesFinderResult";

        [ActivityInput(
            Hint = "SearchFileName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string SearchFileName
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "ResultVariableName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string ResultVariableName
        {
            get => GetState(() => DefaultFileParameterName);
            set => SetState(value);
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            BaseDirectory ??= context.GetVariable<string>(BaseDirectoryVariableName)!;

            LogInput(() => SearchFileName);
            LogInput(() => ResultVariableName);
            LogInput(() => ExcludeDirectories, string.Join("; ", ExcludeDirectories));
            LogInput(() => BaseDirectory);
            LogInput(() => Overwrite);

            var files = SearchFilesInDirectory(BaseDirectory, SearchFileName, ExcludeDirectories).ToArray();

            if (files.Length == 0) throw new FileNotFoundException();

            context.Output = files;
            context.SetVariable(ResultVariableName, files);
            LogOutput(() => files, $"Found files count: {files.Length}, stored in parameter: '{ResultVariableName}'");

            return Done();
        }
    }
}