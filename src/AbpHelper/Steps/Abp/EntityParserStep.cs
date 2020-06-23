using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DosSEdo.AbpHelper.Extensions;
using DosSEdo.AbpHelper.Models;
using DosSEdo.AbpHelper.Steps.Common;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace DosSEdo.AbpHelper.Steps.Abp
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
            string entityFile = await context.EvaluateAsync(EntityFile, cancellationToken);
            LogInput(() => entityFile);
            ProjectInfo projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");

            string sourceText = await File.ReadAllTextAsync(entityFile);

            try
            {
                Microsoft.CodeAnalysis.SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                if (root.ContainsDiagnostics)
                {
                    // source contains syntax error
                    ParseException ex = new ParseException(root.GetDiagnostics().Select(diag => diag.ToString()));
                    throw ex;
                }

                string @namespace = root.Descendants<NamespaceDeclarationSyntax>().Single().Name.ToString();
                string relativeDirectory = @namespace.RemovePreFix(projectInfo.FullName + ".").Replace('.', '/');
                ClassDeclarationSyntax classDeclarationSyntax = root.Descendants<ClassDeclarationSyntax>().Single();
                string className = classDeclarationSyntax.Identifier.ToString();
                BaseListSyntax baseList = classDeclarationSyntax.BaseList!;
                GenericNameSyntax genericNameSyntax = baseList.Descendants<SimpleBaseTypeSyntax>()
                    .First(node => !node.ToFullString().StartsWith("I")) // Not interface
                    .Descendants<GenericNameSyntax>()
                    .FirstOrDefault();

                string baseType;
                string? primaryKey;
                IEnumerable<string>? keyNames = null;
                if (genericNameSyntax == null)
                {
                    // No generic parameter -> Entity with Composite Keys
                    baseType = baseList.Descendants<SimpleBaseTypeSyntax>().Single().Type.ToString();
                    primaryKey = null;

                    // Get composite keys
                    MethodDeclarationSyntax getKeysMethod = root.Descendants<MethodDeclarationSyntax>().Single(m => m.Identifier.ToString() == "GetKeys");
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

                List<PropertyInfo> properties = root.Descendants<PropertyDeclarationSyntax>()
                        .Select(prop => new PropertyInfo(prop.Type.ToString(), prop.Identifier.ToString()))
                        .ToList()
                    ;

                EntityInfo entityInfo = new EntityInfo(@namespace, className, baseType, primaryKey, relativeDirectory);
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
                    foreach (string error in pe.Errors)
                        Logger.LogError(error);
                throw;
            }
        }
    }
}