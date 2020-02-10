using AbpHelper.Steps.Abp.ModificationCreatorSteps;
using AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Services;

namespace AbpHelper.Workflow.Abp
{
    public static class ServiceGenerationWorkflow
    {
        public static IActivityBuilder AddServiceGenerationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    /* Generate dto, service interface and class files */
                    .Then<TemplateGroupGenerationStep>(
                        step => { step.GroupName = "Service"; }
                    )
                    /* Add mapping */
                    .Then<FileFinderStep>(step => step.SearchFileName = new LiteralExpression("*ApplicationAutoMapperProfile.cs"))
                    .Then<ApplicationAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}