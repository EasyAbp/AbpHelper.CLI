using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Expressions;
using Elsa.Services;

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
                            step.TemplateName = "UsingEntityNamespace";
                            step.GeneratedTextKey = new LiteralExpression("EntityUsingText");
                        }
                    ).WithName(name)
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