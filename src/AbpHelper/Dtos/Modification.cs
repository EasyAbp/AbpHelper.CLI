namespace AbpHelper.Dtos
{
    public abstract class Modification
    {
        public int StartLine { get; }

        protected Modification(int startLine)
        {
            StartLine = startLine;
        }
    }

    public class Insertion : Modification
    {
        public string Content { get; }
        public InsertPosition InsertPosition { get; }

        public Insertion(int startLine, string content, InsertPosition insertPosition = InsertPosition.Before) : base(startLine)
        {
            Content = content;
            InsertPosition = insertPosition;
        }
    }

    public enum InsertPosition
    {
        Before,
        After
    }

    public class Deletion : Modification
    {
        public int EndLine { get; }

        public Deletion(int startLine, int endLine) : base(startLine)
        {
            EndLine = endLine;
        }
    }

    public class Replacement : Modification
    {
        public int EndLine { get; }
        public string Content { get; }

        public Replacement(int startLine, int endLine, string content) : base(startLine)
        {
            EndLine = endLine;
            Content = content;
        }
    }
}