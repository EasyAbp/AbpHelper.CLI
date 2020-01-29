using System;
using System.Threading.Tasks;
using AbpHelper.Dtos;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AbpHelper.Parsers
{
    public class EntityParser : IEntityParser
    {
        public Task<EntityInfo> Parse(string text)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            throw new NotImplementedException();
        }
    }
}