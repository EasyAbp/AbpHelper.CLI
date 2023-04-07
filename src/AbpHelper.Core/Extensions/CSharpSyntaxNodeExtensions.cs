﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EasyAbp.AbpHelper.Core.Extensions
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

        public static bool DescendantsContain<T>(this CSharpSyntaxNode node, string text) where T : CSharpSyntaxNode
        {
            return node.Descendants<T>().Any(child => ContainsIgnoreWhitespace(child.ToFullString(), text));
        }

        public static bool Contains(this CSharpSyntaxNode node, string text)
        {
            return ContainsIgnoreWhitespace(node.ToFullString(), text);
        }
        
        public static bool DescendantsNotContain<T>(this CSharpSyntaxNode node, string text) where T : CSharpSyntaxNode
        {
            return node.Descendants<T>().All(child => !ContainsIgnoreWhitespace(child.ToFullString(), text));
        }

        public static bool NotContains(this CSharpSyntaxNode node, string text)
        {
            return !ContainsIgnoreWhitespace(node.ToFullString(), text);
        }

        public static string GetDocument(this CSharpSyntaxNode node)
        {
            return node.GetLeadingTrivia()
                      .Where(p => p.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia)
                      .Select(p => p.GetDocumentText())
                      .JoinAsString(";");
        }

        private static string GetDocumentText(this SyntaxTrivia syntaxTrivia)
        {
            return syntaxTrivia.ToString()
                .Replace("/// <summary>\r\n", string.Empty)
                .Replace("/// <summary>", string.Empty)
                .Replace("<summary>\r\n", string.Empty)
                .Replace("<summary>", string.Empty)
                .Replace("/// </summary>\r\n", string.Empty)
                .Replace("/// </summary>", string.Empty)
                .Replace("</summary>\r\n", string.Empty)
                .Replace("</summary>", string.Empty)
                .Trim()
                .Replace("/// ", string.Empty)
                .Replace("///", string.Empty)
                .Replace("\r\n", string.Empty);
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