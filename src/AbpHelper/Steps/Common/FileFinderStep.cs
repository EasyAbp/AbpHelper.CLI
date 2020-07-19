using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyAbp.AbpHelper.Steps.Common
{
    public class FileFinderStep : Step
    {
        public const string DefaultFileParameterName = "FileFinderResult";

        public WorkflowExpression<string> BaseDirectory
        {
            get => GetState(() => new JavaScriptExpression<string>("BaseDirectory"));
            set => SetState(value);
        }

        public WorkflowExpression<string> IgnoreDirectories
        {
            get => GetState(() => new JavaScriptExpression<string>("IgnoreDirectories"));
            set => SetState(value);
        }

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
            var ignoredDirectories = await context.EvaluateAsync(IgnoreDirectories, cancellationToken);
            LogInput(() => ignoredDirectories);
            var searchFileName = await context.EvaluateAsync(SearchFileName, cancellationToken);
            LogInput(() => searchFileName);
            var errorIfNotFound = await context.EvaluateAsync(ErrorIfNotFound, cancellationToken);
            LogInput(() => errorIfNotFound);

            var ignored = ignoredDirectories?
                              .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(x => Path.Combine(baseDirectory, x)).ToArray() ?? Array.Empty<string>();

            var files = Directory.EnumerateFiles(baseDirectory, searchFileName, SearchOption.AllDirectories)
                .Where(x => !ignored.Any(x.StartsWith))
                .ToArray();

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