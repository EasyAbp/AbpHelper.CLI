using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Newtonsoft.Json.Linq;

namespace EasyAbp.AbpHelper.Core.Steps.Abp
{
    public class LocalizationJsonModificationCreatorStep : Step
    {
        public WorkflowExpression<string> TargetFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        public WorkflowExpression<string> LocalizationJson
        {
            get => GetState<WorkflowExpression<string>>();
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var targetFile = await context.EvaluateAsync(TargetFile, cancellationToken);
            LogInput(() => targetFile);
            var localizations = await context.EvaluateAsync(LocalizationJson, cancellationToken);
            var jNew = JObject.Parse(localizations);

            var jsonText = await File.ReadAllTextAsync(targetFile);
            var jDoc = JObject.Parse(jsonText);
            var jTexts = jDoc["texts"] ?? jDoc["Texts"]!;

            foreach (var kv in jNew)
                if (jTexts[kv.Key] == null) // Prevent inserting duplicate localization
                    jTexts[kv.Key] = kv.Value;

            await File.WriteAllTextAsync(targetFile, jDoc.ToString());

            return Done();
        }
    }
}