using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using EasyAbp.AbpHelper.Models;
using EasyParser.Core;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.Typescript
{
    public class AppRoutingModuleStep : TypeScriptModificationCreatorStep
    {
        public AppRoutingModuleStep(TextParser textParser) : base(textParser)
        {
        }

        protected override IList<ModificationBuilder<IParseTree>> CreateModifications(WorkflowExecutionContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}