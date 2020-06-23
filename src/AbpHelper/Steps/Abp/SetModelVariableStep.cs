using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Models;
using Elsa.Results;
using Elsa.Services.Models;

namespace DosSEdo.AbpHelper.Steps.Abp
{
    public class SetModelVariableStep : Step
    {
        protected override Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            ProjectInfo projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");
            object option = context.GetVariable<object>("Option");
            EntityInfo entityInfo = context.GetVariable<EntityInfo>("EntityInfo");
            ServiceInfo serviceInfo = context.GetVariable<ServiceInfo>("ServiceInfo");
            IEnumerable<KeyValuePair<string, Elsa.Models.Variable>> variables = context.GetVariables().Where(v => v.Key.StartsWith("Bag."));
            ExpandoObject bag = new ExpandoObject();
            foreach (KeyValuePair<string, Elsa.Models.Variable> variable in variables)
            {
                ((IDictionary<string, object>) bag)[variable.Key.RemovePreFix("Bag.")] = variable.Value.Value;
            }

            context.SetVariable("Model", new
            {
                ProjectInfo = projectInfo,
                Option = option,
                EntityInfo = entityInfo,
                ServiceInfo = serviceInfo,
                Bag = bag,
            });
            return Task.FromResult(Done());
        }
    }
}