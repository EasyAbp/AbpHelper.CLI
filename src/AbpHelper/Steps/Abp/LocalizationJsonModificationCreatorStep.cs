using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Newtonsoft.Json.Linq;

namespace DosSEdo.AbpHelper.Steps.Abp
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
            string targetFile = await context.EvaluateAsync(TargetFile, cancellationToken);
            LogInput(() => targetFile);
            string localizations = await context.EvaluateAsync(LocalizationJson, cancellationToken);
            JObject jNew = JObject.Parse(localizations);

            string jsonText = await File.ReadAllTextAsync(targetFile);
            JObject jDoc = JObject.Parse(jsonText);
            JToken jTexts = jDoc["texts"] ?? jDoc["Texts"]!;

            foreach (System.Collections.Generic.KeyValuePair<string, JToken?> kv in jNew)
                if (jTexts[kv.Key] == null) // Prevent inserting duplicate localization
                    jTexts[kv.Key] = kv.Value;

            await File.WriteAllTextAsync(targetFile, jDoc.ToString());

            return Done();
        }
    }
}