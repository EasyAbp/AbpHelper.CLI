using Microsoft.CodeAnalysis;

namespace EasyAbp.AbpHelper.Core.Extensions
{
    public static class SyntaxTokenExtensions
    {
        public static int GetStartLine(this SyntaxToken node)
        {
            return node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        }

        public static int GetEndLine(this SyntaxToken node)
        {
            return node.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
        }
    }
}