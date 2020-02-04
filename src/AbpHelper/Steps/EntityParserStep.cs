using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace AbpHelper.Steps
{
    public class EntityParserStep : Step
    {
        protected override Task RunStep()
        {
            var entitySourceFile = GetParameter<string>("FilePathName");
            LogInput(() => entitySourceFile);

            var sourceText = File.ReadAllText(entitySourceFile);

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

                SetParameter("EntityInfo", entityInfo);
                LogOutput(() => entityInfo);
                return Task.CompletedTask;
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

    public class ParseException : Exception
    {
        public ParseException(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }

        public List<string> Errors { get; } = new List<string>();
    }
}