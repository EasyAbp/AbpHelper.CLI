using System;
using System.IO;
using EasyAbp.AbpHelper.Core.Extensions;
using Microsoft.Extensions.FileProviders;
using Scriban;
using Scriban.Runtime;
using Volo.Abp.DependencyInjection;
using Volo.Abp.VirtualFileSystem;

namespace EasyAbp.AbpHelper.Core.Generator
{
    public class TextGenerator : ISingletonDependency
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IVirtualFileProvider _virtualFileProvider;

        public TextGenerator(
            ITemplateLoader templateLoader,
            IVirtualFileProvider virtualFileProvider)
        {
            _templateLoader = templateLoader;
            _virtualFileProvider = virtualFileProvider;
        }

        public string GenerateByTemplateName(string templateDirectory, string templateName, object model)
        {
            return GenerateByTemplateName(templateDirectory, templateName, model, out _);
        }
        
        public string GenerateByTemplateName(string templateDirectory, string templateName, object model, out TemplateContext context)
        {
            string path = Path.Combine(templateDirectory, templateName).NormalizePath();
            var templateFile = _virtualFileProvider.GetFileInfo(path);
            var templateText = templateFile.ReadAsString();
            return GenerateByTemplateText(templateText, model, out context);
        }

        public string GenerateByTemplateText(string templateText, object model, string? templateDirectory = null)
        {
            return GenerateByTemplateText(templateText, model, out _);
        }

        public string GenerateByTemplateText(string templateText, object model, out TemplateContext context)
        {
            context = new TemplateContext();
            var scriptObject = new ScriptObject();
            scriptObject.SetValue("abp", new AbpFunctions(), true);
            scriptObject.Import(model, renamer: member => member.Name);
            context.PushGlobal(scriptObject);
            context.MemberRenamer = member => member.Name;
            context.TemplateLoader = _templateLoader;

            var template = Template.Parse(templateText);
            var text = template.Render(context).Replace("\r\n", Environment.NewLine);
            return text;
        }
    }
}