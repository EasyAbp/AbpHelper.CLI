using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace EasyAbp.AbpHelper.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static int GetStartLine(this SyntaxNode node)
        {
            return node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        }

        public static int GetEndLine(this SyntaxNode node)
        {
            return node.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
        }

        public static IEnumerable<T> Descendants<T>(this SyntaxNode node) where T : SyntaxNode
        {
            return node.DescendantNodes().OfType<T>();
        }

        public static bool DescendantsNotContain<T>(this SyntaxNode node, string text) where T : SyntaxNode
        {
            return node.Descendants<T>().All(child => !child.ToFullString().Contains(text));
        }
    }
}