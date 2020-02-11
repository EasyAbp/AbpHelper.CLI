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
            var baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);
            var searchFileName = await context.EvaluateAsync(SearchFileName, cancellationToken);
            LogInput(() => SearchFileName);
            var resultParameterName = await context.EvaluateAsync(ResultVariableName, cancellationToken);

            var files = Directory.EnumerateFiles(baseDirectory, searchFileName, SearchOption.AllDirectories).ToArray();

            var filePathName = files.SingleOrDefault();
            if (filePathName == null) throw new FileNotFoundException();

            context.SetLastResult(filePathName);
            context.SetVariable(resultParameterName, filePathName);
            LogOutput(() => filePathName, $"Found file: '{filePathName}', stored in parameter: '{ResultVariableName}'");

            return Done();
        }
    }
}