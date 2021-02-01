using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    public class MultiFileFinderStep : StepWithOption
    {
        public const string DefaultFileParameterName = "MultiFilesFinderResult";

        public WorkflowExpression<string> SearchFileName
        {
            get => GetState<WorkflowExpression<string>>();
            set => SetState(value);
        }

        public WorkflowExpression<string> ResultVariableName
        {
            get => GetState(() => new LiteralExpression(DefaultFileParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);
            var excludeDirectories = await context.EvaluateAsync(ExcludeDirectories, cancellationToken);
            LogInput(() => excludeDirectories, string.Join("; ", excludeDirectories));
            var searchFileName = await context.EvaluateAsync(SearchFileName, cancellationToken);
            LogInput(() => SearchFileName);
            var resultParameterName = await context.EvaluateAsync(ResultVariableName, cancellationToken);

            var files = SearchFilesInDirectory(baseDirectory, searchFileName, excludeDirectories).ToArray();

            if (files.Length == 0) throw new FileNotFoundException();

            context.SetLastResult(files);
            context.SetVariable(resultParameterName, files);
            LogOutput(() => files, $"Found files count: {files.Length}, stored in parameter: '{ResultVariableName}'");

            return Done();
        }
    }
}