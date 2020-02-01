using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class TextGenerationStep : StepBase
    {
        protected override Task RunStep()
        {
            var templateFile = GetParameter<string>("TemplateFile");
            var model = GetParameter<object>("Model");
            LogInput(() => templateFile);
            LogInput(() => model);
            // TODO: Use template engine to generate text
            var text = "CODE";

            SetParameter("Text", text);
            LogOutput(() => text);
            return Task.CompletedTask;
        }
    }
}