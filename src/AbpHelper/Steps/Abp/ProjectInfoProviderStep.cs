using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Models;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;

namespace DosSEdo.AbpHelper.Steps.Abp
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
            string baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);

            TemplateType templateType;
            if (Directory.EnumerateFiles(baseDirectory, "*.DbMigrator.csproj", SearchOption.AllDirectories).Any())
                templateType = TemplateType.Application;
            else if (Directory.EnumerateFiles(baseDirectory, "*.Host.Shared.csproj", SearchOption.AllDirectories).Any())
                templateType = TemplateType.Module;
            else
                throw new NotSupportedException($"Unknown ABP project structure. Directory: {baseDirectory}");

            // Assume the domain project must be existed for an ABP project
            string domainCsprojFile = Directory.EnumerateFiles(baseDirectory, "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (domainCsprojFile == null) throw new NotSupportedException($"Cannot find the domain project file. Make sure it is a valid ABP project. Directory: {baseDirectory}");

            string fileName = Path.GetFileName(domainCsprojFile);
            string fullName = fileName.RemovePostFix(".Domain.csproj");

            UiFramework uiFramework;
            if (Directory.EnumerateFiles(baseDirectory, "*.cshtml", SearchOption.AllDirectories).Any())
            {
                uiFramework = UiFramework.RazorPages;
                if (templateType == TemplateType.Application)
                {
                    context.SetVariable("AspNetCoreDir", Path.Combine(baseDirectory, "aspnet-core"));
                }
                else
                {
                    context.SetVariable("AspNetCoreDir", baseDirectory);
                }
            }
            else if (Directory.EnumerateFiles(baseDirectory, "app.module.ts", SearchOption.AllDirectories).Any())
            {
                uiFramework = UiFramework.Angular;
                context.SetVariable("AspNetCoreDir", Path.Combine(baseDirectory, "aspnet-core"));
            }
            else
            {
                uiFramework = UiFramework.None;
                context.SetVariable("AspNetCoreDir", baseDirectory);
            }

            bool tiered = false;
            if (templateType == TemplateType.Application) tiered = Directory.EnumerateFiles(baseDirectory, "*.IdentityServer.csproj", SearchOption.AllDirectories).Any();

            ProjectInfo projectInfo = new ProjectInfo(baseDirectory, fullName, templateType, uiFramework, tiered);
            context.SetLastResult(projectInfo);
            context.SetVariable("ProjectInfo", projectInfo);
            LogOutput(() => projectInfo);

            return Done();
        }
    }
}