using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp;
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
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("*DbContextModelCreatingExtensions.cs")
                    )
                    .Then<DbContextModelCreatingExtensionsStep>()
                    .Then<FileModifierStep>()
                ;
        }
    }
}