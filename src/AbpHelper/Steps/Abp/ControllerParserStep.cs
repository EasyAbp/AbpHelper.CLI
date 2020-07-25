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
    // TODO: Refactor this, merged with ServiceInterfaceSemanticParserStep into one class 
    public class ControllerParserStep : Step
    {
        public WorkflowExpression<string> ControllerFile
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var appServiceInterfaceFile = await context.EvaluateAsync(ControllerFile, cancellationToken);
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
                var compilation = CSharpCompilation.Create("Controller")
                    .AddReferences(
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                    )
                    .AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)))
                    .AddSyntaxTrees(tree);

                var usings = root.Descendants<UsingDirectiveSyntax>().Select(@using => @using.Name.ToString());
                var @namespace = root.Descendants<NamespaceDeclarationSyntax>().Single().Name.ToString();
                var relativeDirectory = @namespace.RemovePreFix(projectInfo.FullName + ".").Replace('.', '/');
                var classDeclarationSyntax = root.Descendants<ClassDeclarationSyntax>().Single();
                var className = classDeclarationSyntax.Identifier.ToString();

                var model = compilation.GetSemanticModel(tree);
                var symbol = model.GetDeclaredSymbol(classDeclarationSyntax)!;
                var methods = symbol
                        .GetBaseTypesAndThis()
                        .SelectMany(type => type.GetMembers())
                        .Where(type => type.Kind == SymbolKind.Method)
                        .Cast<IMethodSymbol>()
                        .Select(SymbolExtensions.ToMethodInfo)
                    ;

                var controllerInfo = new ClassInfo(@namespace, className, relativeDirectory);
                controllerInfo.Usings.AddRange(usings);
                controllerInfo.Methods.AddRange(methods);

                context.SetLastResult(controllerInfo);
                context.SetVariable("ControllerInfo", controllerInfo);
                LogOutput(() => controllerInfo);

                return Done();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Parsing controller failed.");
                if (e is ParseException pe)
                    foreach (var error in pe.Errors)
                        Logger.LogError(error);
                throw;
            }
        }
    }
}