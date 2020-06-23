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
    public class FileFinderStep : Step
    {
        public const string DefaultFileParameterName = "FileFinderResult";

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
            LogInput(() => searchFileName);
            string resultParameterName = await context.EvaluateAsync(ResultVariableName, cancellationToken);

            string[] files = Directory.EnumerateFiles(baseDirectory, searchFileName, SearchOption.AllDirectories).ToArray();

            string filePathName = files.SingleOrDefault();
            if (filePathName == null) throw new FileNotFoundException(searchFileName);

            context.SetLastResult(filePathName);
            context.SetVariable(resultParameterName, filePathName);
            LogOutput(() => filePathName, $"Found file: '{filePathName}', stored in parameter: '{ResultVariableName}'");

            return Done();
        }
    }
}