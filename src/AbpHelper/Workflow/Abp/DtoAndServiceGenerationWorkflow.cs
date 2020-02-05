using System.IO;
using AbpHelper.Extensions;
using AbpHelper.Generator;
using AbpHelper.Models;
using AbpHelper.Steps;

namespace AbpHelper.Workflow.Abp
{
    public static class DtoAndServiceGenerationWorkflow
    {
        public static WorkflowBuilder AddDtoAndServiceGenerationWorkflow(this WorkflowBuilder builder)
        {
            return builder
                    .AddStep<DirectoryFinderStep>(
                        step => step.SearchDirectoryName = "*.Application.Contracts"
                    )
                    .AddStep<FileGenerationStep>(
                        step =>
                        {
                            var entityInfo = step.Get<EntityInfo>();
                            var contractDir = step.GetParameter<string>("DirectoryPathName");
                            step.File = Path.Combine(contractDir, $"{entityInfo.ClassName}s", $"{entityInfo.ClassName}Dto");
                            step.Contents = TextGenerator.Generate("Dtos", new {EntityInfo = entityInfo});
                        })
                ;
        }
    }
}