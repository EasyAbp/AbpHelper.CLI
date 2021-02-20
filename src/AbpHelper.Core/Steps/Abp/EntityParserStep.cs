using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Core.Steps.Abp
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
            var projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");

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
                var relativeDirectory = @namespace.RemovePreFix(projectInfo.FullName + ".").Replace('.', '/');
                var classDeclarationSyntax = root.Descendants<ClassDeclarationSyntax>().Single();
                var className = classDeclarationSyntax.Identifier.ToString();
                var baseList = classDeclarationSyntax.BaseList!;
                var genericNameSyntax = baseList.Descendants<SimpleBaseTypeSyntax>()
                    .First(node => !node.ToFullString().StartsWith("I")) // Not interface
                    .Descendants<GenericNameSyntax>()
                    .FirstOrDefault();

                string baseType;
                string? primaryKey;
                IEnumerable<string>? keyNames = null;
                if (genericNameSyntax == null)
                {
                    // No generic parameter -> Entity with Composite Keys
                    baseType = baseList.Descendants<SimpleBaseTypeSyntax>().Single(node => !node.ToFullString().StartsWith("I")).Type.ToString();
                    primaryKey = null;

                    // Get composite keys
                    var getKeysMethod = root.Descendants<MethodDeclarationSyntax>().Single(m => m.Identifier.ToString() == "GetKeys");
                    keyNames = getKeysMethod
                            .Descendants<InitializerExpressionSyntax>()
                            .First()
                            .Descendants<IdentifierNameSyntax>()
                            .Select(id => id.Identifier.ToString())
                        ;
                }
                else
                {
                    // Normal entity
                    baseType = genericNameSyntax.Identifier.ToString();
                    primaryKey = genericNameSyntax.Descendants<TypeArgumentListSyntax>().Single().Arguments[0].ToString();
                }

                var properties = root.Descendants<PropertyDeclarationSyntax>()
                        .Select(prop => new PropertyInfo(prop.Type.ToString(), prop.Identifier.ToString()))
                        .ToList()
                    ;
                var entityInfo = new EntityInfo(@namespace, className, baseType, primaryKey, relativeDirectory);
                entityInfo.Properties.AddRange(properties);
                if (keyNames != null)
                {
                    entityInfo.CompositeKeyName = $"{className}Key";
                    entityInfo.CompositeKeys.AddRange(
                        keyNames.Select(k => properties.Single(prop => prop.Name == k)));
                }

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
}