using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Generate.Crud
{
    public static class EntityConstructorsGenerationWorkflow
    {
        public static IActivityBuilder AddEntityConstructorsGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    .Then<EntityConstructorsStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}