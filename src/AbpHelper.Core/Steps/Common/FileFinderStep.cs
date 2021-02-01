using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    public class FileFinderStep : StepWithOption
    {
        public const string DefaultFileParameterName = "FileFinderResult";

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

        public WorkflowExpression<bool> ErrorIfNotFound
        {
            get => GetState(() => new JavaScriptExpression<bool>("true"));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var resultVariableName = await context.EvaluateAsync(ResultVariableName, cancellationToken);
            var baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);
            var excludeDirectories = await context.EvaluateAsync(ExcludeDirectories, cancellationToken);
            LogInput(() => excludeDirectories, string.Join("; ", excludeDirectories));
            var searchFileName = await context.EvaluateAsync(SearchFileName, cancellationToken);
            LogInput(() => searchFileName);
            var errorIfNotFound = await context.EvaluateAsync(ErrorIfNotFound, cancellationToken);
            LogInput(() => errorIfNotFound);

            var files = SearchFilesInDirectory(baseDirectory, searchFileName, excludeDirectories);

            var filePathName = files.SingleOrDefault();

            context.SetLastResult(filePathName);
            context.SetVariable(resultVariableName, filePathName);
            if (filePathName == null)
            {
                if (errorIfNotFound) throw new FileNotFoundException(searchFileName);
                LogOutput(() => filePathName, $"File: '{filePathName}' not found, stored 'null' in parameter: '{ResultVariableName}'");
            }
            else
            {
                LogOutput(() => filePathName, $"Found file: '{filePathName}', stored in parameter: '{ResultVariableName}'");
            }

            return Done();
        }
    }
}