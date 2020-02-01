using System.Threading.Tasks;
using AbpHelper.Models;

namespace AbpHelper.Steps
{
    public class TextGenerationStep : StepBase
    {
        public TextGenerationStep(WorkflowContext context) : base(context)
        {
        }

        public override Task Run()
        {
            var templateFile = GetParameter<string>("TemplateFile");
            var model = GetParameter<object>("Model");
            // TODO: Use template engine to generate text
            var text = "CODE";

            SetParameter("Text", text);
            return Task.CompletedTask;
        }
    }
}