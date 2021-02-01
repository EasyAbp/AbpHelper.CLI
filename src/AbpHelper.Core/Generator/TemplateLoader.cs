using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.AbpHelper.Core.Generator
{
    public class TemplateLoader : ITemplateLoader, ISingletonDependency
    {
        private readonly IFileProvider _fileProvider;

        public TemplateLoader(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            return templateName;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return _fileProvider.GetFileInfo(templatePath).ReadAsString();
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return await _fileProvider.GetFileInfo(templatePath).ReadAsStringAsync();
        }
    }
}