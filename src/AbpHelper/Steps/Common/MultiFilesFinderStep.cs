using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace DosSEdo.AbpHelper.Steps.Common
{
    public class MultiFileFinderStep : Step
    {
        public const string DefaultFileParameterName = "MultiFilesFinderResult";

        public WorkflowExpression<string> BaseDirectory
        {
            get => GetState(() => new JavaScriptExpression<string>("BaseDirectory"));
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
            string baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);
            string searchFileName = await context.EvaluateAsync(SearchFileName, cancellationToken);
            LogInput(() => SearchFileName);
            string resultParameterName = await context.EvaluateAsync(ResultVariableName, cancellationToken);

            string[] files = Directory.EnumerateFiles(baseDirectory, searchFileName, SearchOption.AllDirectories).ToArray();

            if (files.Length == 0) throw new FileNotFoundException();

            context.SetLastResult(files);
            context.SetVariable(resultParameterName, files);
            LogOutput(() => files, $"Found files count: {files.Length}, stored in parameter: '{ResultVariableName}'");

            return Done();
        }
    }
}