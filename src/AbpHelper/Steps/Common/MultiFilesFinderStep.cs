using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Steps.Common
{
    public class MultiFileFinderStep : Step
    {
        public const string DefaultFileParameterName = "MultiFilesFinderResult";

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

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);
            var ignoredDirectories = await context.EvaluateAsync(IgnoreDirectories, cancellationToken);
            LogInput(() => ignoredDirectories);
            var searchFileName = await context.EvaluateAsync(SearchFileName, cancellationToken);
            LogInput(() => SearchFileName);
            var resultParameterName = await context.EvaluateAsync(ResultVariableName, cancellationToken);

            var ignored = ignoredDirectories?
                              .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(x => Path.Combine(baseDirectory, x)).ToArray() ?? Array.Empty<string>();

            var files = Directory.EnumerateFiles(baseDirectory, searchFileName, SearchOption.AllDirectories)
                .Where(x => !ignored.Any(x.StartsWith))
                .ToArray();

            if (files.Length == 0) throw new FileNotFoundException();

            context.SetLastResult(files);
            context.SetVariable(resultParameterName, files);
            LogOutput(() => files, $"Found files count: {files.Length}, stored in parameter: '{ResultVariableName}'");

            return Done();
        }
    }
}