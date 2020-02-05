namespace AbpHelper.Models
{
    public abstract class Modification
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
    }

    public enum InsertPosition
    {
        Before,
        After
    }

    public class Deletion : Modification
    {
        public Deletion(int startLine, int endLine) : base(startLine)
        {
            EndLine = endLine;
        }

        public int EndLine { get; }
    }

    public class Replacement : Modification
    {
        public Replacement(int startLine, int endLine, string contents) : base(startLine)
        {
            EndLine = endLine;
            Contents = contents;
        }

        public int EndLine { get; }

        public string Contents { get; }
    }
}