using System;
using System.IO;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Abp
{
    [Activity(
        Category = "ProjectInfoProviderStep",
        Description = "ProjectInfoProviderStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class ProjectInfoProviderStep : StepWithOption
    {
        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            BaseDirectory ??= context.GetVariable<string>(BaseDirectoryVariableName)!;
            
            LogInput(() => ExcludeDirectories, string.Join("; ", ExcludeDirectories));
            LogInput(() => BaseDirectory);

            TemplateType templateType;
            if (FileExistsInDirectory(BaseDirectory, "*.Host.Shared.csproj", ExcludeDirectories))
            {
                templateType = TemplateType.Module;
            }
            else if (FileExistsInDirectory(BaseDirectory, "*.DbMigrator.csproj", ExcludeDirectories))
            {
                templateType = TemplateType.Application;
            }
            else
            {
                throw new NotSupportedException($"Unknown ABP project structure. Directory: {BaseDirectory}");
            }

            // Assume the domain project must be existed for an ABP project
            var domainCsprojFile = SearchFileInDirectory(BaseDirectory, "*.Domain.csproj", ExcludeDirectories);
            if (domainCsprojFile == null)
                throw new NotSupportedException(
                    $"Cannot find the domain project file. Make sure it is a valid ABP project. Directory: {BaseDirectory}");

            var fileName = Path.GetFileName(domainCsprojFile);
            var fullName = fileName.RemovePostFix(".Domain.csproj");

            UiFramework uiFramework;
            if (FileExistsInDirectory(BaseDirectory, "*.cshtml", ExcludeDirectories))
            {
                uiFramework = UiFramework.RazorPages;
            }
            else if (FileExistsInDirectory(BaseDirectory, "app.module.ts", ExcludeDirectories))
            {
                uiFramework = UiFramework.Angular;
            }
            else
            {
                uiFramework = UiFramework.None;
            }

            var aspNetCoreDir = Path.Combine(BaseDirectory, "aspnet-core");
            if (Directory.Exists(aspNetCoreDir))
            {
                context.SetVariable(VariableNames.AspNetCoreDir, aspNetCoreDir);
            }
            else
            {
                context.SetVariable(VariableNames.AspNetCoreDir, BaseDirectory);
            }

            EnsureSlnFileExists(context, fullName);

            var tiered = false;
            if (templateType == TemplateType.Application)
            {
                tiered = FileExistsInDirectory(BaseDirectory, "*.IdentityServer.csproj", ExcludeDirectories);
            }

            var projectInfo = new ProjectInfo(BaseDirectory, fullName, templateType, uiFramework, tiered);

            context.Output = projectInfo;
            context.SetVariable("ProjectInfo", projectInfo);
            LogOutput(() => projectInfo);

            return Done();
        }

        private void EnsureSlnFileExists(ActivityExecutionContext context, string projectName)
        {
            var aspNetCoreDir = context.GetVariable<string>(VariableNames.AspNetCoreDir)!;
            var slnFile = Path.Combine(aspNetCoreDir, $"{projectName}.sln");
            if (!File.Exists(slnFile))
            {
                throw new FileNotFoundException(
                    $"The solution file '{projectName}.sln' is not found in '{aspNetCoreDir}'. Make sure you specific the right folder.");
            }
        }
    }
}