using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class EfCoreConfigurationWorkflow
    {
        public static IActivityBuilder AddEfCoreConfigurationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    /* Add entity property to DbContext */
                    .Then<FileFinderStep>(
                        step => { step.SearchFileName = new JavaScriptExpression<string>("`${ProjectInfo.Name}DbContext.cs`"); })
                    .Then<TextGenerationStep>(step => { step.TemplateName = "DbContext_Property"; })
                    .Then<DbContextStep>()
                    .Then<FileModifierStep>()
                    /* Add entity configuration to DbContextModelCreatingExtensions */
                    .Then<TextGenerationStep>(step =>
                    {
                        step.TemplateName = "DbContextModelCreatingExtensions_Using";
                        step.GeneratedTextKey = new LiteralExpression("ModelingUsingText");
                    })
                    .Then<TextGenerationStep>(step =>
                    {
                        step.TemplateName = "DbContextModelCreatingExtensions_EntityConfig";
                        step.GeneratedTextKey = new LiteralExpression("EntityConfigText");
                    })
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("*DbContextModelCreatingExtensions.cs")
                    )
                    .Then<DbContextModelCreatingExtensionsStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}