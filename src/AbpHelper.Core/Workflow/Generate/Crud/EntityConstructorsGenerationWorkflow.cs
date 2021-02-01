using EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
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