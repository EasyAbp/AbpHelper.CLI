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
        Category = "DirectoryFinderStep",
        Description = "DirectoryFinderStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class DirectoryFinderStep : StepWithOption
    {
        public const string DefaultDirectoryParameterName = "DirectoryFinderResult";

        [ActivityInput(
            Hint = "SearchDirectoryName",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string SearchDirectoryName
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
            get => GetState(() => DefaultDirectoryParameterName);
            set => SetState(value);
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            BaseDirectory ??= context.GetVariable<string>(BaseDirectoryVariableName)!;

            LogInput(() => SearchDirectoryName);
            LogInput(() => ResultVariableName);
            LogInput(() => ExcludeDirectories, string.Join("; ", ExcludeDirectories));
            LogInput(() => BaseDirectory);
            LogInput(() => Overwrite);

            var directoryPathName = SearchDirectoryInDirectory(BaseDirectory, SearchDirectoryName, ExcludeDirectories);
            context.Output = directoryPathName;
            context.SetVariable(ResultVariableName, directoryPathName);
            LogOutput(() => directoryPathName,
                $"Found directory: {directoryPathName}, stored in parameter: [{ResultVariableName}]");

            return Done();
        }
    }
}