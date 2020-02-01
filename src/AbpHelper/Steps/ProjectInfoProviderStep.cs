using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Models;
using AbpHelper.Workflow;

namespace AbpHelper.Steps
{
    public class ProjectInfoProviderStep : StepBase
    {
        public ProjectInfoProviderStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        protected override Task RunStep()
        {
            var projectBaseDirectory = GetParameter<string>("ProjectBaseDirectory");
            LogInput(() => projectBaseDirectory);

            TemplateType templateType;
            if (Directory.EnumerateFiles(projectBaseDirectory, "*.DbMigrator.csproj", SearchOption.AllDirectories).Any())
                templateType = TemplateType.Application;
            else if (Directory.EnumerateFiles(projectBaseDirectory, "*.Web.Unified.csproj", SearchOption.AllDirectories).Any())
                templateType = TemplateType.Module;
            else
                throw new NotSupportedException($"Unknown ABP project structure. Directory: {projectBaseDirectory}");

            // Assume the domain project must be existed for an ABP project
            var domainCsprojFile = Directory.EnumerateFiles(projectBaseDirectory, "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (domainCsprojFile == null) throw new NotSupportedException($"Cannot find the domain project file. Make sure it is a valid ABP project. Directory: {projectBaseDirectory}");

            var fileName = Path.GetFileNameWithoutExtension(domainCsprojFile);
            var fullName = fileName.RemovePostFix(".Domain.csproj");
            var name = fullName.Split('.').Last();

            UIFramework uiFramework;
            if (Directory.EnumerateFiles(projectBaseDirectory, "*.cshtml", SearchOption.AllDirectories).Any())
                uiFramework = UIFramework.RazorPages;
            else if (Directory.EnumerateFiles(projectBaseDirectory, "app.module.ts", SearchOption.AllDirectories).Any())
                uiFramework = UIFramework.Angular;
            else
                uiFramework = UIFramework.None;

            var tiered = false;
            if (templateType == TemplateType.Application) tiered = Directory.EnumerateFiles(projectBaseDirectory, "*.IdentityServer.csproj").Any();

            var projectInfo = new ProjectInfo(projectBaseDirectory, fullName, name, templateType, uiFramework, tiered);
            SetParameter("ProjectInfo", projectInfo);
            LogOutput(() => projectInfo);
            return Task.CompletedTask;
        }
    }
}