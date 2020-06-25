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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Steps.Abp
{
    // TODO: Remove ServiceInterfaceParserStep?
    public class ServiceInterfaceSemanticParserStep : Step
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

                // Scan "{ProjectInfo.FullName}.*.dll" and "Volo.*.dll", add them to the compilation later
                var dlls = Directory.EnumerateFiles(projectInfo.BaseDirectory, "*.dll", SearchOption.AllDirectories)
                        .Where(dll =>
                        {
                            string fileName = Path.GetFileName(dll);
                            return fileName.StartsWith("Volo.") || fileName.StartsWith(projectInfo.FullName);
                        })
                    ;
                // Create compilation of the interface
                var compilation = CSharpCompilation.Create("ServiceInterface")
                    .AddReferences(
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                    )
                    .AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)))
                    .AddSyntaxTrees(tree);

                var @namespace = root.Descendants<NamespaceDeclarationSyntax>().Single().Name.ToString();
                var relativeDirectory = @namespace.RemovePreFix(projectInfo.FullName + ".").Replace('.', '/');
                var interfaceDeclarationSyntax = root.Descendants<InterfaceDeclarationSyntax>().Single();
                var interfaceName = interfaceDeclarationSyntax.Identifier.ToString();

                var model = compilation.GetSemanticModel(tree);
                var symbol = model.GetDeclaredSymbol(interfaceDeclarationSyntax)!;
                var methods = symbol
                        .AllInterfaces
                        .Add(symbol)
                        .SelectMany(type => type.GetMembers())
                        .Where(type => type.Kind == SymbolKind.Method)
                        .Cast<IMethodSymbol>()
                        .Select(CreateMethodInfo)
                    ;

                var serviceInfo = new ServiceInfo(@namespace, interfaceName, relativeDirectory);
                serviceInfo.Methods.AddRange(methods);

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

        private MethodInfo CreateMethodInfo(IMethodSymbol methodSymbol)
        {
            string returnType = methodSymbol.ReturnType.ToMinimalQualifiedName();
            var methodInfo = new MethodInfo(
                methodSymbol.DeclaredAccessibility.ToString().ToLower(),
                returnType,
                methodSymbol.Name
                );
            methodInfo.Parameters.AddRange(
                methodSymbol.Parameters
                    .Select(ps => new ParameterInfo(
                        ps.Type.ToMinimalQualifiedName(),
                        ps.Name)
                    )
            );
            return methodInfo;
        }
    }
}