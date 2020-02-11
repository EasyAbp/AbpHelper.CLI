using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Steps.Abp
{
    public class EntityParserStep : Step
    {
        public WorkflowExpression<string> EntityFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var entityFile = await context.EvaluateAsync(EntityFile, cancellationToken);
            LogInput(() => entityFile);

            var sourceText = await File.ReadAllTextAsync(entityFile);

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
                var baseList = classDeclarationSyntax.BaseList!;
                var genericNameSyntax = baseList.Descendants<SimpleBaseTypeSyntax>().Single().Descendants<GenericNameSyntax>().FirstOrDefault();
                string baseType;
                string? primaryKey;
                if (genericNameSyntax == null)
                {
                    // No generic parameter -> Entity with Composite Keys
                    baseType = baseList.Descendants<SimpleBaseTypeSyntax>().Single().Type.ToString();
                    primaryKey = null;
                }
                else
                {
                    // Normal entity
                    baseType = genericNameSyntax.Identifier.ToString();
                    primaryKey = genericNameSyntax.Descendants<TypeArgumentListSyntax>().Single().Arguments[0].ToString();
                }

                var properties = root.Descendants<PropertyDeclarationSyntax>()
                    .Select(prop => new PropertyInfo(prop.Type.ToString(), prop.Identifier.ToString()));

                var entityInfo = new EntityInfo(@namespace, className, baseType, primaryKey);
                entityInfo.Properties.AddRange(properties);

                context.SetLastResult(entityInfo);
                context.SetVariable("EntityInfo", entityInfo);
                LogOutput(() => entityInfo);

                return Done();
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