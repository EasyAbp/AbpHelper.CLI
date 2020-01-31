using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class TextGenerationStep : IStep<TextGenerationStepInput, TextGenerationStepOutput>
    {
        public Task<TextGenerationStepOutput> Run(TextGenerationStepInput input)
        {
            var templateFile = input.TemplateFile;
            var model = input.Model;
            // TODO: Use template engine to generate text
            var text = "CODE";

            return Task.FromResult(new TextGenerationStepOutput(text));
        }
    }

    public class TextGenerationStepInput
    {
        public TextGenerationStepInput(string templateFile, object model)
        {
            TemplateFile = templateFile;
            Model = model;
        }

        public string TemplateFile { get; }

        public object Model { get; }
    }

    public class TextGenerationStepOutput
    {
        public TextGenerationStepOutput(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}