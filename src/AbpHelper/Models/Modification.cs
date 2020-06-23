namespace DosSEdo.AbpHelper.Models
{
    public interface IStartLine
    {
        int StartLine { get; }
    }

    public interface IRange : IStartLine
    {
        int EndLine { get; }
    }

    public abstract class Modification : IStartLine
    {
        protected Modification(int startLine)
        {
            StartLine = startLine;
        }

        public int StartLine { get; }
    }

    public class Insertion : Modification
    {
        public Insertion(int startLine, string contents, InsertPosition insertPosition = InsertPosition.Before) : base(startLine)
        {
            Contents = contents;
            InsertPosition = insertPosition;
        }

        public string Contents { get; }

        public InsertPosition InsertPosition { get; }

        public override string ToString()
        {
            return $"Insertion: {nameof(StartLine)}: {StartLine}, {nameof(InsertPosition)}: {InsertPosition}";
        }
    }

    public enum InsertPosition
    {
        Before,
        After
    }

    public class Deletion : Modification, IRange
    {
        public Deletion(int startLine, int endLine) : base(startLine)
        {
            EndLine = endLine;
        }

        public int EndLine { get; }

        public override string ToString()
        {
            return $"Deletion: {nameof(StartLine)}: {StartLine}, {nameof(EndLine)}: {EndLine}";
        }
    }

    public class Replacement : Modification, IRange
    {
        public Replacement(int startLine, int endLine, string contents) : base(startLine)
        {
            EndLine = endLine;
            Contents = contents;
        }

        public string Contents { get; }

        public int EndLine { get; }

        public override string ToString()
        {
            return $"Replacement: {nameof(StartLine)}: {StartLine}, {nameof(EndLine)}: {EndLine}";
        }
    }
}