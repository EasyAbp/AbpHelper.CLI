using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Builders;

namespace EasyAbp.AbpHelper.Core.Workflow.Generate.Crud
{
    public static class EntityUsingGenerationWorkflow
    {
        public static IActivityBuilder AddEntityUsingGenerationWorkflow(this IActivityBuilder builder, string name)
        {
            return builder
                    .Then<TextGenerationStep>(
                        step =>
                        {
                            step.Set(x => x.TemplateName, "UsingEntityNamespace");
                            step.Set(x => x.GeneratedTextKey, "EntityUsingText");
                        }
                    ).WithName(name)
                    .Then<TextGenerationStep>(
                        step =>
                        {
                            step.Set(x => x.TemplateName, "UsingEntityDtoNamespace");
                            step.Set(x => x.GeneratedTextKey, "EntityDtoUsingText");
                        }
                    )
                ;
        }
    }
}