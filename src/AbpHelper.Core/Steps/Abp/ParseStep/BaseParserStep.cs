using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using TypeInfo = EasyAbp.AbpHelper.Core.Models.TypeInfo;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ParseStep
{
    public abstract class BaseParserStep<TType> : Step where TType : TypeDeclarationSyntax
    {
        public WorkflowExpression<string> File
        {
            get => GetState(() => new JavaScriptExpression<string>(FileFinderStep.DefaultFileParameterName));
            set => SetState(value);
        }

        public abstract WorkflowExpression<string> OutputVariableName { get; set; }

        protected abstract IEnumerable<MethodInfo> GetMethodInfos(TType typeDeclarationSyntax, INamedTypeSymbol typeSymbol);

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var file = await context.EvaluateAsync(File, cancellationToken);
            LogInput(() => file);
            string outputVariableName = await context.EvaluateAsync(OutputVariableName, cancellationToken);
            LogInput(() => outputVariableName);
            var projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo");
            var sourceText = await System.IO.File.ReadAllTextAsync(file, cancellationToken);

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
                // Create compilation of the TType
                var compilation = CSharpCompilation.Create(outputVariableName)
                    .AddReferences(
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                    )
                    .AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)))
                    .AddSyntaxTrees(tree);

                var usings = root.Descendants<UsingDirectiveSyntax>().Select(@using => @using.Name.ToString());
                var @namespace = root.Descendants<NamespaceDeclarationSyntax>().Single().Name.ToString();
                var relativeDirectory = @namespace.RemovePreFix(projectInfo.FullName + ".").Replace('.', '/');
                var typeDeclarationSyntax = root.Descendants<TType>().Single();
                var typeName = typeDeclarationSyntax.Identifier.ToString();
                var attributes = typeDeclarationSyntax.Descendants<AttributeListSyntax>().Select(attr => attr.ToString());
                var model = compilation.GetSemanticModel(tree);
                var typeSymbol = model.GetDeclaredSymbol(typeDeclarationSyntax)!;
                var methods = GetMethodInfos(typeDeclarationSyntax, typeSymbol);

                var typeInfo = new TypeInfo(@namespace, typeName, relativeDirectory);
                typeInfo.Usings.AddRange(usings);
                typeInfo.Attributes.AddRange(attributes);
                typeInfo.Methods.AddRange(methods);

                context.SetLastResult(typeInfo);
                context.SetVariable(outputVariableName, typeInfo);
                LogOutput(() => typeInfo);

                return Done();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Parsing {outputVariableName} failed.");
                if (e is ParseException pe)
                    foreach (var error in pe.Errors)
                        Logger.LogError(error);
                throw;
            }
        }


        protected MethodInfo GetMethodInfoFromSymbol(IMethodSymbol symbol)
        {
            var methodInfo = new MethodInfo(
                symbol.DeclaredAccessibility.ToString().ToLower(),
                symbol.ReturnType.ToMinimalQualifiedName(),
                symbol.ReturnType.ToDisplayString(),
                symbol.Name
            );

            methodInfo.Parameters.AddRange(
                symbol.Parameters
                    .Select(ps => new ParameterInfo(ps)
                    )
            );

            methodInfo.Attributes.AddRange(
                symbol.GetAttributes()
                    .Where(attr => attr.AttributeClass?.Name != nameof(AsyncStateMachineAttribute))
                    .Select(attr => attr.ToString()!)
            );
            return methodInfo;
        }
    }
}