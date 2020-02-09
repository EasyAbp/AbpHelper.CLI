using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Scriban;
using Scriban.Runtime;

namespace AbpHelper.Generator
{
    public static class TextGenerator
    {
        public static string GenerateByTemplateName(string templateName, object model)
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory!;
            var templateFile = Path.Combine(appDir, "Templates", templateName + ".sbntxt");
            var templateText = File.ReadAllText(templateFile);
            return GenerateByTemplateText(templateText, model);
        }

        public static string GenerateByTemplateText(string templateText, object model)
        {
            var context = new TemplateContext();
            var scriptObject = new ScriptObject();
            scriptObject.SetValue("abp", new AbpFunctions(), true);
            if (model is JObject jObj) model = jObj.ToObject<Dictionary<string, object>>();
            scriptObject.Import(model, renamer: member => member.Name);
            context.PushGlobal(scriptObject);
            context.MemberRenamer = member => member.Name;

            var template = Template.Parse(templateText);
            var text = template.Render(context).Replace("\r\n", Environment.NewLine);
            return text;
        }
    }
}