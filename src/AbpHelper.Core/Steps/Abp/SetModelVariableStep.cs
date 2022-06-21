using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Abp
{
    [Activity(
        Category = "SetModelVariableStep",
        Description = "SetModelVariableStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class SetModelVariableStep : Step
    {
        protected override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            var projectInfo = context.GetVariable<ProjectInfo?>("ProjectInfo");
            var option = context.GetVariable<object?>("Option");
            var entityInfo = context.GetVariable<EntityInfo?>("EntityInfo");
            var interfaceInfo = context.GetVariable<TypeInfo?>("InterfaceInfo");
            var classInfo = context.GetVariable<TypeInfo?>("ClassInfo");
            var dtoInfo = context.GetVariable<DtoInfo?>("DtoInfo");
            var variables = context.WorkflowExecutionContext.GetMergedVariables().Data
                .Where(v => v.Key.StartsWith("Bag."));
            var bag = new ExpandoObject();

            foreach (var variable in variables)
            {
                ((IDictionary<string, object?>) bag)[variable.Key.RemovePreFix("Bag.")] = variable.Value;
            }

            context.SetVariable("Model", new
            {
                ProjectInfo = projectInfo,
                Option = option,
                EntityInfo = entityInfo,
                InterfaceInfo = interfaceInfo,
                ClassInfo = classInfo,
                Bag = bag,
                DtoInfo = dtoInfo,
            });

            return new ValueTask<IActivityExecutionResult>(Done());
        }
    }
}