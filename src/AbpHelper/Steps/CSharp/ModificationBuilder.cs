using System;
using System.Threading;
using System.Threading.Tasks;
using AbpHelper.Models;
using Elsa.Expressions;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace AbpHelper.Steps.CSharp
{
    public abstract class ModificationBuilder
    {
        public ModificationBuilder(Func<CSharpSyntaxNode, int> startLineExpression, Func<CSharpSyntaxNode, bool>? modifyCondition)
        {
            StartLineExpression = startLineExpression;
            ModifyCondition = modifyCondition ?? (node => true);
        }

        public Func<CSharpSyntaxNode, int> StartLineExpression { get; }
        public Func<CSharpSyntaxNode, bool> ModifyCondition { get; }

        public abstract Task<Modification> Build(CSharpSyntaxNode root, WorkflowExecutionContext context);
    }

    public class InsertionBuilder : ModificationBuilder
    {
        public InsertionBuilder(Func<CSharpSyntaxNode, int> startLineExpression, WorkflowExpression<string> contents, InsertPosition insertPosition = InsertPosition.Before, Func<CSharpSyntaxNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            Contents = contents;
            InsertPosition = insertPosition;
        }

        public WorkflowExpression<string> Contents { get; }
        public InsertPosition InsertPosition { get; }

        public override async Task<Modification> Build(CSharpSyntaxNode root, WorkflowExecutionContext context)
        {
            var contents = await context.EvaluateAsync(Contents, CancellationToken.None);
            return new Insertion(StartLineExpression(root), contents, InsertPosition);
        }
    }

    public class DeletionBuilder : ModificationBuilder
    {
        public DeletionBuilder(Func<CSharpSyntaxNode, int> startLineExpression, Func<CSharpSyntaxNode, int> endLineExpression, Func<CSharpSyntaxNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            EndLineExpression = endLineExpression;
        }

        public Func<CSharpSyntaxNode, int> EndLineExpression { get; }

        public override async Task<Modification> Build(CSharpSyntaxNode root, WorkflowExecutionContext context)
        {
            return new Deletion(StartLineExpression(root), EndLineExpression(root));
        }
    }

    public class ReplacementBuilder : ModificationBuilder
    {
        public ReplacementBuilder(Func<CSharpSyntaxNode, int> startLineExpression, Func<CSharpSyntaxNode, int> endLineExpression, WorkflowExpression<string> contents, Func<CSharpSyntaxNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            Contents = contents;
            EndLineExpression = endLineExpression;
        }

        public Func<CSharpSyntaxNode, int> EndLineExpression { get; }
        public WorkflowExpression<string> Contents { get; }

        public override async Task<Modification> Build(CSharpSyntaxNode root, WorkflowExecutionContext context)
        {
            var contents = await context.EvaluateAsync(Contents, CancellationToken.None);
            return new Replacement(StartLineExpression(root), EndLineExpression(root), contents);
        }
    }
}