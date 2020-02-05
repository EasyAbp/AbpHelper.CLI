using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbpHelper.Extensions;
using AbpHelper.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace AbpHelper.Steps
{
    public class EntityParserStep : Step
    {
        public string File { get; set; } = string.Empty;

        protected override async Task RunStep()
        {
            var entitySourceFile = File.IsNullOrEmpty() ? GetParameter<string>("FilePathName") : File;
            LogInput(() => entitySourceFile);

            var sourceText = await System.IO.File.ReadAllTextAsync(entitySourceFile);

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

                var @namespace = root.Descendants<NamespaceDeclarationSyntax>().Single().Name.ToString();
                var classDeclarationSyntax = root.Descendants<ClassDeclarationSyntax>().Single();
                var className = classDeclarationSyntax.Identifier.ToString();
                var baseList = classDeclarationSyntax.BaseList;
                var baseType = baseList?.Descendants<SimpleBaseTypeSyntax>().Single().Descendants<GenericNameSyntax>().Single().Identifier.ToString();
                var primaryKey = baseList?.Descendants<TypeArgumentListSyntax>().Single().Arguments[0].ToString();

                var properties = root.Descendants<PropertyDeclarationSyntax>()
                    .Select(prop => new PropertyInfo(prop.Type.ToString(), prop.Identifier.ToString()));

                var entityInfo = new EntityInfo(@namespace, className, baseType, primaryKey);
                entityInfo.Properties.AddRange(properties);

                SetParameter("EntityInfo", entityInfo);
                LogOutput(() => entityInfo);
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