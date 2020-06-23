using System;

namespace DosSEdo.AbpHelper.Models
{
    public abstract class ModificationBuilder<TNode>
    {
        public ModificationBuilder(Func<TNode, int> startLineExpression, Func<TNode, bool>? modifyCondition)
        {
            StartLineExpression = startLineExpression;
            ModifyCondition = modifyCondition ?? (node => true);
        }

        public Func<TNode, int> StartLineExpression { get; }
        public Func<TNode, bool> ModifyCondition { get; set; }

        public abstract Modification Build(TNode root);
    }

    public class InsertionBuilder<TNode> : ModificationBuilder<TNode>
    {
        public InsertionBuilder(Func<TNode, int> startLineExpression, string contents, InsertPosition insertPosition = InsertPosition.Before, Func<TNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            Contents = contents;
            InsertPosition = insertPosition;
        }

        public string Contents { get; }
        public InsertPosition InsertPosition { get; }

        public override Modification Build(TNode root)
        {
            return new Insertion(StartLineExpression(root), Contents, InsertPosition);
        }
    }

    public class DeletionBuilder<TNode> : ModificationBuilder<TNode>
    {
        public DeletionBuilder(Func<TNode, int> startLineExpression, Func<TNode, int> endLineExpression, Func<TNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            EndLineExpression = endLineExpression;
        }

        public Func<TNode, int> EndLineExpression { get; }

        public override Modification Build(TNode root)
        {
            return new Deletion(StartLineExpression(root), EndLineExpression(root));
        }
    }

    public class ReplacementBuilder<TNode> : ModificationBuilder<TNode>
    {
        public ReplacementBuilder(Func<TNode, int> startLineExpression, Func<TNode, int> endLineExpression, string contents, Func<TNode, bool>? modifyCondition = null) : base(startLineExpression, modifyCondition)
        {
            Contents = contents;
            EndLineExpression = endLineExpression;
        }

        public Func<TNode, int> EndLineExpression { get; }
        public string Contents { get; }

        public override Modification Build(TNode root)
        {
            return new Replacement(StartLineExpression(root), EndLineExpression(root), Contents);
        }
    }
}