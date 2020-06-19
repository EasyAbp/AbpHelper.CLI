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
                    baseType = baseList.Descendants<SimpleBaseTypeSyntax>().Single().Type.ToString();
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

                var allLine = File.ReadAllLines(entityFile);
                var popDic = getPoperLine(allLine);
                var summer = getClassSummer(allLine);

                foreach (var item in properties)
                {
                    if (popDic.ContainsKey(item.Name))
                        item.Summer = popDic[item.Name];
                }

                var entityInfo = new EntityInfo(@namespace, className, baseType, primaryKey, relativeDirectory);
                entityInfo.Properties.AddRange(properties);
                entityInfo.Summer = summer;
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

        private static Dictionary<string, string> getPoperLine(string[] allLine)
        {


            List<string> list = new List<string>();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (var str in allLine)
            {

                if (str.Contains("public") && str.Contains("get") && str.Contains("set"))
                {
                    list.Add(str.Trim());
                }
                else if (str.Contains("///") && !str.Contains("summary"))
                {
                    list.Add(str.Trim());
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains("///") && (i + 1) < list.Count)
                {
                    if (list[i + 1].Contains("get"))
                        dic.Add(list[i + 1].Split(" ")[2], list[i].Trim('/'));
                }
            }

            return dic;
        }

        private static string getClassSummer(string[] allLine)
        {
            string summer = "";

            List<string> list = new List<string>();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (var str in allLine)
            {


                if (str.Contains("///") && !str.Contains("summary"))
                {
                    summer = str.Trim().Trim('/').Trim();
                    break;
                }


            }


            return summer;
        }
    }
}