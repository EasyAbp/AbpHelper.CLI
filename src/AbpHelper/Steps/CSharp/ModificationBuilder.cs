using System;
using AbpHelper.Models;
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
        public Func<CSharpSyntaxNode, bool> ModifyCondition { get; set; }

        public abstract Modification Build(CSharpSyntaxNode root);
    }

    public class InsertionBuilder : ModificationBuilder
    {
        public InsertionBuilder(Func<CSharpSyntaxNode, int> startLineExpression, string contents, InsertPosition insertPosition = InsertPosition.Before, Func<CSharpSyntaxNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            Contents = contents;
            InsertPosition = insertPosition;
        }

        public string Contents { get; }
        public InsertPosition InsertPosition { get; }

        public override Modification Build(CSharpSyntaxNode root)
        {
            return new Insertion(StartLineExpression(root), Contents, InsertPosition);
        }
    }

    public class DeletionBuilder : ModificationBuilder
    {
        public DeletionBuilder(Func<CSharpSyntaxNode, int> startLineExpression, Func<CSharpSyntaxNode, int> endLineExpression, Func<CSharpSyntaxNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            EndLineExpression = endLineExpression;
        }

        public Func<CSharpSyntaxNode, int> EndLineExpression { get; }

        public override Modification Build(CSharpSyntaxNode root)
        {
            return new Deletion(StartLineExpression(root), EndLineExpression(root));
        }
    }

    public class ReplacementBuilder : ModificationBuilder
    {
        public ReplacementBuilder(Func<CSharpSyntaxNode, int> startLineExpression, Func<CSharpSyntaxNode, int> endLineExpression, string contents, Func<CSharpSyntaxNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            Contents = contents;
            EndLineExpression = endLineExpression;
        }

        public Func<CSharpSyntaxNode, int> EndLineExpression { get; }
        public string Contents { get; }

        public override Modification Build(CSharpSyntaxNode root)
        {
            return new Replacement(StartLineExpression(root), EndLineExpression(root), Contents);
        }
    }
}