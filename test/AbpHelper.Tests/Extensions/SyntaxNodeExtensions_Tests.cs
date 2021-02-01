using System.Linq;
using EasyAbp.AbpHelper.Core.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace EasyApp.AbpHelper.Tests.Extensions
{
    public class SyntaxNodeExtensionsTests
    {
        private const string CSharpCode = 
@"using System;

namespace EasyAbp.AbpHelper
{
    public   class Hello
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(""Hello world"");
        }

        public int   MethodA()
        {
        }
    }
}
";

        private static CompilationUnitSyntax GetRootNode(string code)
        {
            var node = CSharpSyntaxTree.ParseText(code);
            return node.GetCompilationUnitRoot();
        }

        [Fact]
        public void GetStartLine_GetEndLine_Test()
        {
            // Arrange
            var root = GetRootNode(CSharpCode);
            var main = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            
            // Act & Assert
            main.GetStartLine().ShouldBe(7);
            main.GetEndLine().ShouldBe(10);
        }
        
        [Fact]
        public void Descendants_Test()
        {
            // Arrange
            var root = GetRootNode(CSharpCode);
            
            // Act
            var main = root.Descendants<MethodDeclarationSyntax>().First();
            
            // Assert
            main.Identifier.Text.ShouldBe("Main");
        }

        [Fact]
        public void DescendantsNotContain_True()
        {
            // Arrange
            var root = GetRootNode(CSharpCode);
            
            // Act
            bool result = root.DescendantsNotContain<MethodDeclarationSyntax>("World");
            
            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void DescendantsNotContain_NormalizeWhitespace()
        {
            // Arrange
            var root = GetRootNode(CSharpCode);
            
            // Act
            bool result = root.DescendantsNotContain<MethodDeclarationSyntax>("int MethodA");
            
            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public void DescendantsNotContain_False()
        {
            // Arrange
            var root = GetRootNode(CSharpCode);
            
            // Act
            bool result = root.DescendantsNotContain<MethodDeclarationSyntax>("Hello world");
            
            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public void NotContains_True()
        {
            // Arrange
            var root = GetRootNode(CSharpCode);
            
            // Act
            bool result = root.NotContains("using System.Linq;");
            
            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void NotContains_NormalizeWhitespace()
        {
            // Arrange
            var root = GetRootNode(CSharpCode);
            
            // Act
            bool result = root.NotContains("public class Hello");
            
            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public void NotContains_False()
        {
            // Arrange
            var root = GetRootNode(CSharpCode);
            
            // Act
            bool result = root.NotContains("using System;");
            
            // Assert
            result.ShouldBeFalse();
        }
        
        /// <summary>
        /// https://github.com/EasyAbp/AbpHelper.CLI/issues/43
        /// </summary>
        [Fact]
        public void Issue_43()
        {
            // Arrange
            string code = @"
        public Todo(
            Guid id,
            string content,
            bool done
        ) : base(id)
        {
            Content = content;
            Done = done;
        }
";
            var root = GetRootNode(code);
            
            // Act
            bool result = root.NotContains("public Todo(Guid id,string content,bool done):base(id ) {Content = content; Done=done;}");
            
            // Assert
            result.ShouldBeFalse();
        }
    }
}