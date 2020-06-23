using DosSEdo.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa.Services;

namespace DosSEdo.AbpHelper.Workflow.Generate.Crud
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