using System;
using System.IO;
using DosSEdo.AbpHelper.Extensions;
using Microsoft.Extensions.FileProviders;
using Scriban;
using Scriban.Runtime;
using Volo.Abp.DependencyInjection;

namespace DosSEdo.AbpHelper.Generator
{
    public class TextGenerator : ISingletonDependency
    {
        private readonly IFileProvider _fileProvider;

        public TextGenerator(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public string GenerateByTemplateName(string templateDirectory, string templateName, object model)
        {
            return GenerateByTemplateName(templateDirectory, templateName, model, out _);
        }
        
        public string GenerateByTemplateName(string templateDirectory, string templateName, object model, out TemplateContext context)
        {
            string path = Path.Combine(templateDirectory, templateName).NormalizePath();
            IFileInfo templateFile = _fileProvider.GetFileInfo(path);
            string templateText = templateFile.ReadAsString();
            return GenerateByTemplateText(templateText, model, out context);
        }

        public string GenerateByTemplateText(string templateText, object model)
        {
            return GenerateByTemplateText(templateText, model, out _);
        }

        public string GenerateByTemplateText(string templateText, object model, out TemplateContext context)
        {
            context = new TemplateContext();
            ScriptObject scriptObject = new ScriptObject();
            scriptObject.SetValue("abp", new AbpFunctions(), true);
            scriptObject.Import(model, renamer: member => member.Name);
            context.PushGlobal(scriptObject);
            context.MemberRenamer = member => member.Name;

            Template template = Template.Parse(templateText);
            string text = template.Render(context).Replace("\r\n", Environment.NewLine);
            return text;
        }
    }
}