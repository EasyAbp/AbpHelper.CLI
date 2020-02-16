using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class ServiceGenerationWorkflow
    {
        public static IActivityBuilder AddServiceGenerationWorkflow(this IActivityBuilder builder, string name)
        {
            return builder
                    /* Generate dto, service interface and class files */
                    .Then<TemplateGroupGenerationStep>(
                        step => { step.GroupName = "Service"; }
                    ).WithName(name)
                    /* Add mapping */
                    .Then<FileFinderStep>(step => step.SearchFileName = new LiteralExpression("*ApplicationAutoMapperProfile.cs"))
                    .Then<ApplicationAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}