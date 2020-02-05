using AbpHelper.Extensions;
using AbpHelper.Models;
using AbpHelper.Steps;

namespace AbpHelper.Workflow.Abp
{
    public static class ServiceGenerationWorkflow
    {
        public static WorkflowBuilder AddServiceGenerationWorkflow(this WorkflowBuilder builder)
        {
            return builder
                    .AddStep<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.Model = new
                            {
                                EntityInfo = step.Get<EntityInfo>(),
                                ProjectInfo = step.Get<ProjectInfo>()
                            };
                            step.Overwrite = false;
                            step.GroupName = "Service";
                            step.TargetDirectory = step.GetParameter<string>("BaseDirectory");
                        }
                    )
                ;
        }
    }
}