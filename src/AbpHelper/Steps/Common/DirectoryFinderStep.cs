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
    public class DirectoryFinderStep : Step
    {
        public const string DefaultDirectoryParameterName = "DirectoryFinderResult";

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
            var ignoredDirectories = await context.EvaluateAsync(IgnoreDirectories, cancellationToken);
            LogInput(() => ignoredDirectories);
            LogInput(() => SearchDirectoryName);
            var resultParameterName = await context.EvaluateAsync(ResultVariableName, cancellationToken);

            var ignored = ignoredDirectories?
                              .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(x => Path.Combine(baseDirectory, x)).ToArray() ?? Array.Empty<string>();

            var directoryPathName = Directory.EnumerateDirectories(baseDirectory, SearchDirectoryName, SearchOption.AllDirectories).Single(x => !ignored.Any(x.StartsWith));
            context.SetLastResult(directoryPathName);
            context.SetVariable(resultParameterName, directoryPathName);
            LogOutput(() => directoryPathName, $"Found directory: {directoryPathName}, stored in parameter: [{ResultVariableName}]");

            return Done();
        }
    }
}