using System;
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
    public class ServiceInterfaceParserStep : Step
    {
        public WorkflowExpression<string> ServiceInterfaceFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var appServiceInterfaceFile = await context.EvaluateAsync(ServiceInterfaceFile, cancellationToken);
            LogInput(() => appServiceInterfaceFile);
            var projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");

            var sourceText = await File.ReadAllTextAsync(appServiceInterfaceFile);

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
                var interfaceDeclarationSyntax = root.Descendants<InterfaceDeclarationSyntax>().Single();
                var interfaceName = interfaceDeclarationSyntax.Identifier.ToString();
                int methodsCount = root.Descendants<MethodDeclarationSyntax>().Count();

                var serviceInfo = new ServiceInfo(@namespace, interfaceName, methodsCount, relativeDirectory);

                context.SetLastResult(serviceInfo);
                context.SetVariable("ServiceInfo", serviceInfo);
                LogOutput(() => serviceInfo);

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