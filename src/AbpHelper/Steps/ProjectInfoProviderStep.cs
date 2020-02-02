using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Models;
using AbpHelper.Workflow;

namespace AbpHelper.Steps
{
    public class ProjectInfoProviderStep : Step
    {
        public ProjectInfoProviderStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public string ProjectBaseDirectory { get; set; } = string.Empty;

        protected override Task RunStep()
        {
            LogInput(() => ProjectBaseDirectory);

            TemplateType templateType;
            if (Directory.EnumerateFiles(ProjectBaseDirectory, "*.DbMigrator.csproj", SearchOption.AllDirectories).Any())
                templateType = TemplateType.Application;
            else if (Directory.EnumerateFiles(ProjectBaseDirectory, "*.Web.Unified.csproj", SearchOption.AllDirectories).Any())
                templateType = TemplateType.Module;
            else
                throw new NotSupportedException($"Unknown ABP project structure. Directory: {ProjectBaseDirectory}");

            // Assume the domain project must be existed for an ABP project
            var domainCsprojFile = Directory.EnumerateFiles(ProjectBaseDirectory, "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (domainCsprojFile == null) throw new NotSupportedException($"Cannot find the domain project file. Make sure it is a valid ABP project. Directory: {ProjectBaseDirectory}");

            var fileName = Path.GetFileNameWithoutExtension(domainCsprojFile);
            var fullName = fileName.RemovePostFix(".Domain.csproj");
            var name = fullName.Split('.').Last();

            UIFramework uiFramework;
            if (Directory.EnumerateFiles(ProjectBaseDirectory, "*.cshtml", SearchOption.AllDirectories).Any())
                uiFramework = UIFramework.RazorPages;
            else if (Directory.EnumerateFiles(ProjectBaseDirectory, "app.module.ts", SearchOption.AllDirectories).Any())
                uiFramework = UIFramework.Angular;
            else
                uiFramework = UIFramework.None;

            var tiered = false;
            if (templateType == TemplateType.Application) tiered = Directory.EnumerateFiles(ProjectBaseDirectory, "*.IdentityServer.csproj").Any();

            var projectInfo = new ProjectInfo(ProjectBaseDirectory, fullName, name, templateType, uiFramework, tiered);
            SetParameter("ProjectInfo", projectInfo);
            LogOutput(() => projectInfo);
            return Task.CompletedTask;
        }
    }
}