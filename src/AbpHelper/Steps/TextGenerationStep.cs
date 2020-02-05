using System.Threading.Tasks;
using AbpHelper.Generator;

namespace AbpHelper.Steps
{
    public class TextGenerationStep : Step
    {
        public string TemplateName { get; set; } = string.Empty;
        public object Model { get; set; } = new object();
        public string GeneratedTextKey { get; set; } = "GeneratedText";

        protected override Task RunStep()
        {
            LogInput(() => TemplateName);
            LogInput(() => Model);
            LogInput(() => GeneratedTextKey);

            var text = TextGenerator.GenerateByTemplateName(TemplateName, Model);

            LogOutput(() => text, $"Length: {text.Length}");
            SetParameter(GeneratedTextKey, text);
            return Task.CompletedTask;
        }
    }
}