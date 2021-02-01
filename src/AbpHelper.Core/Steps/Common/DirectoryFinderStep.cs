using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    public class DirectoryFinderStep : StepWithOption
    {
        public const string DefaultDirectoryParameterName = "DirectoryFinderResult";

        public string SearchDirectoryName
        {
            get => GetState<string>();
            set => SetState(value);
        }

        public WorkflowExpression<string> ResultVariableName
        {
            get => GetState(() => new LiteralExpression(DefaultDirectoryParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);
            var excludeDirectories = await context.EvaluateAsync(ExcludeDirectories, cancellationToken);
            LogInput(() => excludeDirectories, string.Join("; ", excludeDirectories));
            LogInput(() => SearchDirectoryName);
            var resultParameterName = await context.EvaluateAsync(ResultVariableName, cancellationToken);


            var directoryPathName = SearchDirectoryInDirectory(baseDirectory, SearchDirectoryName, excludeDirectories);
            context.SetLastResult(directoryPathName);
            context.SetVariable(resultParameterName, directoryPathName);
            LogOutput(() => directoryPathName, $"Found directory: {directoryPathName}, stored in parameter: [{ResultVariableName}]");

            return Done();
        }
    }
}