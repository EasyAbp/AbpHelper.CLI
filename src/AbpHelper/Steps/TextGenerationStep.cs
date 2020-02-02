using System;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Workflow;
using Scriban;

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

            var appDir = AppDomain.CurrentDomain.BaseDirectory!;
            var templateFile = Path.Combine(appDir, "Templates", TemplateFile + ".sbntxt");
            var templateText = File.ReadAllText(templateFile);
            var template = Template.Parse(templateText);
            var text = template.Render(Model, member => member.Name).Replace("\r\n", Environment.NewLine);

            LogOutput(() => text, $"Length: {text.Length}");
            SetParameter("GeneratedText", text);
            return Task.CompletedTask;
        }
    }
}