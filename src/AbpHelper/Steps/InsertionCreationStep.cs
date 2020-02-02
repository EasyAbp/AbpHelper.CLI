using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AbpHelper.Workflow;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AbpHelper.Steps
{
    public class InsertionCreationStep : StepBase
    {
        public InsertionCreationStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        protected override Task RunStep()
        {
            var startLineExpr = GetParameter<Expression<Func<CompilationUnitSyntax, int>>>("StartLineExpression");
            return Task.CompletedTask;
        }
    }
}