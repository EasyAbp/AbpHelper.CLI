using System;
using AbpHelper.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace AbpHelper.Steps.CSharp
{
    public abstract class ModificationBuilder
    {
        public ModificationBuilder(Func<CSharpSyntaxNode, int> startLineExpression, Func<CSharpSyntaxNode, bool>? shouldModifier)
        {
            StartLineExpression = startLineExpression;
            ShouldModifier = shouldModifier ?? (node => true);
        }

        public Func<CSharpSyntaxNode, int> StartLineExpression { get; }
        public Func<CSharpSyntaxNode, bool> ShouldModifier { get; }

        public abstract Modification Build(CSharpSyntaxNode root);
    }

    public class InsertionBuilder : ModificationBuilder
    {
        public InsertionBuilder(Func<CSharpSyntaxNode, int> startLineExpression, string content, InsertPosition insertPosition = InsertPosition.Before, Func<CSharpSyntaxNode, bool>? shouldModifier = null) : base(startLineExpression, shouldModifier)
        {
            Content = content;
            InsertPosition = insertPosition;
        }

        public string Content { get; }
        public InsertPosition InsertPosition { get; }

        public override Modification Build(CSharpSyntaxNode root)
        {
            return new Insertion(StartLineExpression(root), Content, InsertPosition);
        }
    }

    public class DeletionBuilder : ModificationBuilder
    {
        public DeletionBuilder(Func<CSharpSyntaxNode, int> startLineExpression, Func<CSharpSyntaxNode, int> endLineExpression, Func<CSharpSyntaxNode, bool>? shouldModifier = null) : base(startLineExpression, shouldModifier)
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
        public ReplacementBuilder(Func<CSharpSyntaxNode, int> startLineExpression, Func<CSharpSyntaxNode, int> endLineExpression, string content, Func<CSharpSyntaxNode, bool>? shouldModifier = null) : base(startLineExpression, shouldModifier)
        {
            Content = content;
            EndLineExpression = endLineExpression;
        }

        public Func<CSharpSyntaxNode, int> EndLineExpression { get; }
        public string Content { get; }

        public override Modification Build(CSharpSyntaxNode root)
        {
            return new Replacement(StartLineExpression(root), EndLineExpression(root), Content);
        }
    }
}