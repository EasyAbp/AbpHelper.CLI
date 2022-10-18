using System;
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
        Category = "FileFinderStep",
        Description = "FileFinderStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class FileFinderStep : StepWithOption
    {
        public const string DefaultFileParameterName = "FileFinderResult";

        [ActivityInput(
            Hint = "SearchFileName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string SearchFileName
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "ResultVariableName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string ResultVariableName
        {
            get => GetState<string>(() => DefaultFileParameterName);
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "ErrorIfNotFound",
            UIHint = ActivityInputUIHints.Checkbox,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public bool ErrorIfNotFound
        {
            get => GetState(() => true);
            set => SetState(value);
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            BaseDirectory ??= context.GetVariable<string>(BaseDirectoryVariableName)!;

            LogInput(() => SearchFileName);
            LogInput(() => ResultVariableName);
            LogInput(() => ErrorIfNotFound);
            LogInput(() => ExcludeDirectories, string.Join("; ", ExcludeDirectories));
            LogInput(() => BaseDirectory);
            LogInput(() => Overwrite);

            var files = SearchFilesInDirectory(BaseDirectory, SearchFileName, ExcludeDirectories);

            var filePathName = files.SingleOrDefault();

            context.Output = filePathName ?? string.Empty;
            context.SetVariable(ResultVariableName, filePathName);
            if (filePathName == null)
            {
                if (ErrorIfNotFound) throw new FileNotFoundException(SearchFileName);
                LogOutput(() => filePathName,
                    $"File: '{filePathName}' not found, stored 'null' in parameter: '{ResultVariableName}'");
            }
            else
            {
                LogOutput(() => filePathName,
                    $"Found file: '{filePathName}', stored in parameter: '{ResultVariableName}'");
            }

            return Done(filePathName);
        }
    }
}