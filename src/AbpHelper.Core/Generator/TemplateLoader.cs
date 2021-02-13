using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Volo.Abp.DependencyInjection;
using Volo.Abp.VirtualFileSystem;

namespace EasyAbp.AbpHelper.Core.Generator
{
    public class TemplateLoader : ITemplateLoader, ISingletonDependency
    {
        private readonly IVirtualFileProvider _virtualFileProvider;

        public TemplateLoader(IVirtualFileProvider virtualFileProvider)
        {
            _virtualFileProvider = virtualFileProvider;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            return templateName;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return _virtualFileProvider.GetFileInfo(templatePath).ReadAsString();
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return await _virtualFileProvider.GetFileInfo(templatePath).ReadAsStringAsync();
        }
    }
}