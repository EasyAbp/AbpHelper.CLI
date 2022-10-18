using System.IO;
using System.Threading.Tasks;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;
using Newtonsoft.Json.Linq;

namespace EasyAbp.AbpHelper.Core.Steps.Abp
{
    [Activity(
        Category = "LocalizationJsonModificationCreatorStep",
        Description = "LocalizationJsonModificationCreatorStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class LocalizationJsonModificationCreatorStep : Step
    {
        [ActivityInput(
            Hint = "TargetFile",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string TargetFile
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "LocalizationJson",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string LocalizationJson
        {
            get => GetState<string>()!;
            set => SetState(value);
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            LogInput(() => TargetFile);
            var jNew = JObject.Parse(LocalizationJson);

            var jsonText = await File.ReadAllTextAsync(TargetFile);
            var jDoc = JObject.Parse(jsonText);
            var jTexts = jDoc["texts"] ?? jDoc["Texts"]!;

            foreach (var kv in jNew)
                if (jTexts[kv.Key] == null) // Prevent inserting duplicate localization
                    jTexts[kv.Key] = kv.Value;

            await File.WriteAllTextAsync(TargetFile, jDoc.ToString());

            return Done();
        }
    }
}