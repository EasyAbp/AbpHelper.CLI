using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace DosSEdo.AbpHelper.Extensions
{
    public static class CSharpSyntaxNodeExtensions
    {
        public static int GetStartLine(this CSharpSyntaxNode node)
        {
            return node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        }

        public static int GetEndLine(this CSharpSyntaxNode node)
        {
            return node.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
        }

        public static IEnumerable<T> Descendants<T>(this CSharpSyntaxNode node) where T : CSharpSyntaxNode
        {
            return node.DescendantNodes().OfType<T>();
        }

        public static bool DescendantsNotContain<T>(this CSharpSyntaxNode node, string text) where T : CSharpSyntaxNode
        {
            return node.Descendants<T>().All(child => !ContainsIgnoreWhitespace(child.ToFullString(), text));
        }

        public static bool NotContains(this CSharpSyntaxNode node, string text)
        {
            return !ContainsIgnoreWhitespace(node.ToFullString(), text);
        }
        
        private static string RemoveWhitespace(string text)
        {
            return Regex.Replace(text, @"\s", "");
        }

        private static bool ContainsIgnoreWhitespace(string str1, string str2)
        {
            return RemoveWhitespace(str1).Contains(RemoveWhitespace(str2));
        }
    }
}