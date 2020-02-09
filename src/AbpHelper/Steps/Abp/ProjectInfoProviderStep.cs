using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AbpHelper.Models;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace AbpHelper.Steps.Abp
{
    public class ProjectInfoProviderStep : Step
    {
        public WorkflowExpression<string> BaseDirectory
        {
            get => GetState(() => new JavaScriptExpression<string>("BaseDirectory"));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
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
            context.SetLastResult(projectInfo);
            context.SetVariable("ProjectInfo", projectInfo);
            LogOutput(() => projectInfo);

            return Done();
        }
    }
}