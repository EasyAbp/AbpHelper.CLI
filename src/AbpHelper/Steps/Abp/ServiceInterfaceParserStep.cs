using System;
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
    public class ServiceInterfaceParserStep : Step
    {
        public WorkflowExpression<string> ServiceInterfaceFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            string appServiceInterfaceFile = await context.EvaluateAsync(ServiceInterfaceFile, cancellationToken);
            LogInput(() => appServiceInterfaceFile);
            ProjectInfo projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");

            string sourceText = await File.ReadAllTextAsync(appServiceInterfaceFile);

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
                InterfaceDeclarationSyntax interfaceDeclarationSyntax = root.Descendants<InterfaceDeclarationSyntax>().Single();
                string interfaceName = interfaceDeclarationSyntax.Identifier.ToString();
                int methodsCount = root.Descendants<MethodDeclarationSyntax>().Count();

                ServiceInfo serviceInfo = new ServiceInfo(@namespace, interfaceName, methodsCount, relativeDirectory);

                context.SetLastResult(serviceInfo);
                context.SetVariable("ServiceInfo", serviceInfo);
                LogOutput(() => serviceInfo);

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