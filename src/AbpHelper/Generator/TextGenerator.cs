using System;
using System.IO;
using Scriban;
using Scriban.Runtime;

namespace EasyAbp.AbpHelper.Generator
{
    public static class TextGenerator
    {
        public static string GenerateByTemplateName(string templateDirectory, string templateName, object model)
        {
            return GenerateByTemplateName(templateDirectory, templateName, model, out _);
        }
        
        public static string GenerateByTemplateName(string templateDirectory, string templateName, object model, out TemplateContext context)
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory!;
            var templateFile = Path.Combine(appDir, templateDirectory, templateName + ".sbntxt");
            var templateText = File.ReadAllText(templateFile);
            return GenerateByTemplateText(templateText, model, out context);
        }

        public static string GenerateByTemplateText(string templateText, object model)
        {
            return GenerateByTemplateText(templateText, model, out _);
        }

        public static string GenerateByTemplateText(string templateText, object model, out TemplateContext context)
        {
            context = new TemplateContext();
            var scriptObject = new ScriptObject();
            scriptObject.SetValue("abp", new AbpFunctions(), true);
            scriptObject.Import(model, renamer: member => member.Name);
            context.PushGlobal(scriptObject);
            context.MemberRenamer = member => member.Name;

            var template = Template.Parse(templateText);
            var text = template.Render(context).Replace("\r\n", Environment.NewLine);
            return text;
        }
    }
}