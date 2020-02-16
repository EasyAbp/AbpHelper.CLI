using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class CustomRepositoryGenerationWorkflow
    {
        public static IActivityBuilder AddCustomRepositoryGeneration(this IOutcomeBuilder builder)
        {
            return builder
                    /* Generate custom repository interface and class */
                    .Then<TemplateGroupGenerationStep>(
                        step => { step.GroupName = "Repository"; }
                    )
                    /* Add repository configuration to EntityFrameworkCoreModule */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("*EntityFrameworkCoreModule.cs")
                    )
                    .Then<EntityFrameworkCoreModuleStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}