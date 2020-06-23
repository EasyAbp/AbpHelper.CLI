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
    public class DirectoryFinderStep : Step
    {
        public const string DefaultDirectoryParameterName = "DirectoryFinderResult";

        public WorkflowExpression<string> BaseDirectory
        {
            get => GetState(() => new JavaScriptExpression<string>("BaseDirectory"));
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
            string baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);
            LogInput(() => SearchDirectoryName);
            string resultParameterName = await context.EvaluateAsync(ResultVariableName, cancellationToken);

            string directoryPathName = Directory.EnumerateDirectories(baseDirectory, SearchDirectoryName, SearchOption.AllDirectories).Single();
            context.SetLastResult(directoryPathName);
            context.SetVariable(resultParameterName, directoryPathName);
            LogOutput(() => directoryPathName, $"Found directory: {directoryPathName}, stored in parameter: [{ResultVariableName}]");

            return Done();
        }
    }
}