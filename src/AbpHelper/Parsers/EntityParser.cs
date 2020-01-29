using System;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Dtos;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AbpHelper.Parsers
{
    public class EntityParser : IEntityParser
    {
        public ILogger<EntityParser> Logger { get; set; }

        public EntityParser()
        {
            Logger = NullLogger<EntityParser>.Instance;
        }

        public Task<EntityInfo> Parse(string text)
        {
            try
            {
                var tree = CSharpSyntaxTree.ParseText(text);
                var root = tree.GetCompilationUnitRoot();
                string @namespace = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single().Name.ToString();
                var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
                string className = classDeclarationSyntax.Identifier.ToString();
                var baseType = classDeclarationSyntax.BaseList?.Types[0].ToString();

                var properties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                    .Select(prop => new PropertyInfo(prop.Type.ToString(), prop.Identifier.ToString()));

                var entityInfo = new EntityInfo(@namespace, className, baseType);
                foreach (var propertyInfo in properties)
                {
                    entityInfo.Properties.Add(propertyInfo);
                }
                
                return Task.FromResult(entityInfo);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Parsing entity failed.");
                throw;
            }
        }
    }
}