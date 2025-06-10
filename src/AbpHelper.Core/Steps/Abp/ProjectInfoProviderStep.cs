﻿using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Workflow;
using Elsa.Results;
using Elsa.Services.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EasyAbp.AbpHelper.Core.Steps.Abp
{
    public class ProjectInfoProviderStep : StepWithOption
    {
        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context,
            CancellationToken cancellationToken)
        {
            var baseDirectory = await context.EvaluateAsync(BaseDirectory, cancellationToken);
            LogInput(() => baseDirectory);
            var projectName = await context.EvaluateAsync(ProjectName, cancellationToken);
            LogInput(() => projectName);
            var excludeDirectories = await context.EvaluateAsync(ExcludeDirectories, cancellationToken);
            LogInput(() => excludeDirectories, string.Join("; ", excludeDirectories));

            TemplateType templateType;
            if (FileExistsInDirectory(baseDirectory, "*.Host.Shared.csproj", excludeDirectories) ||
                FileExistsInDirectory(baseDirectory, "*.Installer.csproj", excludeDirectories))
            {
                templateType = TemplateType.Module;
            }
            else if (FileExistsInDirectory(baseDirectory, "*.DbMigrator.csproj", excludeDirectories))
            {
                templateType = TemplateType.Application;
            }
            else if (FileExistsInDirectory(baseDirectory, "*.Application.csproj", excludeDirectories) &&
                FileExistsInDirectory(baseDirectory, "*.Application.Contracts.csproj", excludeDirectories) &&
                FileExistsInDirectory(baseDirectory, "*.Domain.csproj", excludeDirectories) &&
                FileExistsInDirectory(baseDirectory, "*.Domain.Shared.csproj", excludeDirectories) &&
                FileExistsInDirectory(baseDirectory, "*.EntityFrameworkCore.csproj", excludeDirectories) &&
                FileExistsInDirectory(baseDirectory, "*.HttpApi.csproj", excludeDirectories)
                )
            {
                templateType = TemplateType.Application;
            }
            else
            {
                throw new NotSupportedException($"Unknown ABP project structure. Directory: {baseDirectory}");
            }

            // Assume the domain project must be existed for an ABP project
            var domainCsprojFile = SearchFileInDirectory(baseDirectory, "*.Domain.csproj", excludeDirectories);
            if (domainCsprojFile == null)
                throw new NotSupportedException(
                    $"Cannot find the domain project file. Make sure it is a valid ABP project. Directory: {baseDirectory}");

            var fileName = Path.GetFileName(domainCsprojFile);
            var fullName = fileName.RemovePostFix(".Domain.csproj");

            UiFramework uiFramework;
            if (FileExistsInDirectory(baseDirectory, "*.cshtml", excludeDirectories))
            {
                uiFramework = UiFramework.RazorPages;
            }
            else if (FileExistsInDirectory(baseDirectory, "app.module.ts", excludeDirectories))
            {
                uiFramework = UiFramework.Angular;
            }
            else
            {
                uiFramework = UiFramework.None;
            }

            string aspNetCoreDir = Path.Combine(baseDirectory, "aspnet-core");
            if (Directory.Exists(aspNetCoreDir))
            {
                context.SetVariable(VariableNames.AspNetCoreDir, aspNetCoreDir);
            }
            else
            {
                context.SetVariable(VariableNames.AspNetCoreDir, baseDirectory);
            }

            EnsureSlnFileExists(context, fullName);

            var tiered = false;
            if (templateType == TemplateType.Application)
            {
                tiered = FileExistsInDirectory(baseDirectory, "*.IdentityServer.csproj", excludeDirectories);
            }

            var projectInfo = new ProjectInfo(baseDirectory, fullName, templateType, uiFramework, tiered, projectName);

            context.SetLastResult(projectInfo);
            context.SetVariable("ProjectInfo", projectInfo);
            LogOutput(() => projectInfo);

            return Done();
        }

        private void EnsureSlnFileExists(WorkflowExecutionContext context, string projectName)
        {
            string aspNetCoreDir = context.GetVariable<string>(VariableNames.AspNetCoreDir);
            string slnFile = Path.Combine(aspNetCoreDir, $"{projectName}.sln");
            string slnxFile = Path.Combine(aspNetCoreDir, $"{projectName}.slnx");
            if (!File.Exists(slnFile) && !File.Exists(slnxFile))
            {
                throw new FileNotFoundException(
                    $"The solution file '{projectName}.sln' is not found in '{aspNetCoreDir}'. Make sure you specific the right folder.");
            }
        }
    }
}