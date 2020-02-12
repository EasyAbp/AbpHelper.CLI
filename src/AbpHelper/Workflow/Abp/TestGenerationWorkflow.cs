﻿using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class TestGenerationWorkflow
    {
        public static IActivityBuilder AddTestGenerationWorkflow(this IActivityBuilder builder)
        {
            return builder
                    /* Generate test files */
                    .Then<TemplateGroupGenerationStep>(
                        step => { step.GroupName = "Test"; }
                    )
                ;
        }
    }
}