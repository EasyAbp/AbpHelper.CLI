using System;
using System.Threading;
using System.Threading.Tasks;
using AbpHelper.Models;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace AbpHelper.Steps.CSharp
{
    public class InsertionStep : Activity
    {
        public Func<CSharpSyntaxNode, int> StartLineExpression
        {
            get => GetState<Func<CSharpSyntaxNode, int>>();
            set => SetState(value);
        }
        
        public Func<CSharpSyntaxNode, bool> ModifyCondition
        {
            get => GetState<Func<CSharpSyntaxNode, bool>>();
            set => SetState(value);
        }

        public WorkflowExpression<string> Contents
        {
            get => GetState<WorkflowExpression<string>>();
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var modification = new Insertion();
        }
        
        protected Modification Build(CSharpSyntaxNode root)
        {
            return new Insertion(StartLineExpression(root), Contents, InsertPosition);
        }
    }
}