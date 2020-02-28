using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Generate.Crud
{
    public static class EntityUsingGenerationWorkflow
    {
        public static IActivityBuilder AddEntityUsingGenerationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    .Then<TextGenerationStep>(
                        step =>
                        {
                            step.TemplateName = "UsingEntityNamespace";
                            step.GeneratedTextKey = new LiteralExpression("EntityUsingText");
                        }
                    )
                    .Then<TextGenerationStep>(
                        step =>
                        {
                            step.TemplateName = "UsingEntityDtoNamespace";
                            step.GeneratedTextKey = new LiteralExpression("EntityDtoUsingText");
                        }
                    )
                ;
        }
    }
}