using System;
using System.Threading.Tasks;
using AbpHelper.Workflow;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AbpHelper.Steps
{
    public class InsertionCreationStep : Step
    {
        public InsertionCreationStep(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public Func<CompilationUnitSyntax, int> StartLineFunc { get; set; } = root => 0;

        protected override Task RunStep()
        {
            return Task.CompletedTask;
        }
    }
}