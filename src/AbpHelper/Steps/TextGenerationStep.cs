using System.Threading.Tasks;
using AbpHelper.Workflow;

namespace AbpHelper.Steps
{
    public class TextGenerationStep : Step
    {
        public TextGenerationStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public string TemplateFile { get; set; } = string.Empty;
        public object Model { get; set; } = new object();

        protected override Task RunStep()
        {
            LogInput(() => TemplateFile);
            LogInput(() => Model);
            // TODO: Use template engine to generate text
            var text = "CODE";

            SetParameter("Text", text);
            LogOutput(() => text, $"Length: {text.Length}");
            return Task.CompletedTask;
        }
    }
}