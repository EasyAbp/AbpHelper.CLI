using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Models;

namespace AbpHelper.Steps
{
    public class ProjectInfoProviderStep : Step
    {
        protected override Task RunStep()
        {
            var baseDirectory = GetParameter<string>("BaseDirectory");
            LogInput(() => baseDirectory);

            TemplateType templateType;
            if (Directory.EnumerateFiles(baseDirectory, "*.DbMigrator.csproj", SearchOption.AllDirectories).Any())
                templateType = TemplateType.Application;
            else if (Directory.EnumerateFiles(baseDirectory, "*.Web.Unified.csproj", SearchOption.AllDirectories).Any())
                templateType = TemplateType.Module;
            else
                throw new NotSupportedException($"Unknown ABP project structure. Directory: {baseDirectory}");

            // Assume the domain project must be existed for an ABP project
            var domainCsprojFile = Directory.EnumerateFiles(baseDirectory, "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (domainCsprojFile == null) throw new NotSupportedException($"Cannot find the domain project file. Make sure it is a valid ABP project. Directory: {baseDirectory}");

            var fileName = Path.GetFileName(domainCsprojFile);
            var fullName = fileName.RemovePostFix(".Domain.csproj");
            var name = fullName.Split('.').Last();

            UIFramework uiFramework;
            if (Directory.EnumerateFiles(baseDirectory, "*.cshtml", SearchOption.AllDirectories).Any())
                uiFramework = UIFramework.RazorPages;
            else if (Directory.EnumerateFiles(baseDirectory, "app.module.ts", SearchOption.AllDirectories).Any())
                uiFramework = UIFramework.Angular;
            else
                uiFramework = UIFramework.None;

            var tiered = false;
            if (templateType == TemplateType.Application) tiered = Directory.EnumerateFiles(baseDirectory, "*.IdentityServer.csproj").Any();

            var projectInfo = new ProjectInfo(baseDirectory, fullName, name, templateType, uiFramework, tiered);
            SetParameter("ProjectInfo", projectInfo);
            LogOutput(() => projectInfo);
            return Task.CompletedTask;
        }
    }
}