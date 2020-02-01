using System;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AbpHelper.Parsers
{
    public class EntityParser : IEntityParser
    {
        public EntityParser()
        {
            Logger = NullLogger<EntityParser>.Instance;
        }

        public ILogger<EntityParser> Logger { get; set; }

        public Task<EntityInfo> Parse(string sourceText)
        {
            try
            {
                var tree = CSharpSyntaxTree.ParseText(sourceText);
                var root = tree.GetCompilationUnitRoot();
                if (root.ContainsDiagnostics)
                {
                    // source contains syntax error
                    var ex = new ParseException(root.GetDiagnostics().Select(diag => diag.ToString()));
                    throw ex;
                }

                var @namespace = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single().Name.ToString();
                var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
                var className = classDeclarationSyntax.Identifier.ToString();
                var baseType = classDeclarationSyntax.BaseList?.Types[0].ToString();

                var properties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                    .Select(prop => new PropertyInfo(prop.Type.ToString(), prop.Identifier.ToString()));

                var entityInfo = new EntityInfo(@namespace, className, baseType);
                entityInfo.Properties.AddRange(properties);

                return Task.FromResult(entityInfo);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Parsing entity failed.");
                if (e is ParseException pe)
                    foreach (var error in pe.Errors)
                        Logger.LogError(error);
                throw;
            }
        }
    }
}