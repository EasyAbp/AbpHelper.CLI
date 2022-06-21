using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
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
        [ActivityInput(
            Hint = "ProjectInfo",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public ProjectInfo? ProjectInfo
        {
            get => GetState<ProjectInfo?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "File",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string? File
        {
            get => GetState<string?>();
            set => SetState(value);
        }

        public abstract string? OutputVariableName { get; set; }

        protected abstract IEnumerable<MethodInfo> GetMethodInfos(TType typeDeclarationSyntax,
            INamedTypeSymbol typeSymbol);

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            File ??= context.GetVariable<string>(FileFinderStep.DefaultFileParameterName)!;
            ProjectInfo ??= context.GetVariable<ProjectInfo>("ProjectInfo")!;

            LogInput(() => File);
            LogInput(() => ProjectInfo);
            LogInput(() => OutputVariableName);

            var sourceText = await System.IO.File.ReadAllTextAsync(File, context.CancellationToken);

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
                var dlls = Directory.EnumerateFiles(ProjectInfo.BaseDirectory, "*.dll", SearchOption.AllDirectories)
                        .Where(dll =>
                        {
                            string fileName = Path.GetFileName(dll);
                            return fileName.StartsWith("Volo.") || fileName.StartsWith(ProjectInfo.FullName);
                        })
                    ;
                // Create compilation of the TType
                var compilation = CSharpCompilation.Create(OutputVariableName)
                    .AddReferences(
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                    )
                    .AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)))
                    .AddSyntaxTrees(tree);

                BaseNamespaceDeclarationSyntax? namespaceSyntax = root
                    .Descendants<NamespaceDeclarationSyntax>()
                    .SingleOrDefault();

                namespaceSyntax ??= root
                    .Descendants<FileScopedNamespaceDeclarationSyntax>()
                    .SingleOrDefault();

                var usings = root.Descendants<UsingDirectiveSyntax>().Select(@using => @using.Name.ToString());
                var @namespace = namespaceSyntax?.Name.ToString()!;
                var relativeDirectory = @namespace.RemovePreFix(ProjectInfo.FullName + ".").Replace('.', '/');
                var typeDeclarationSyntax = root.Descendants<TType>().Single();
                var typeName = typeDeclarationSyntax.Identifier.ToString();
                var attributes = typeDeclarationSyntax.Descendants<AttributeListSyntax>()
                    .Select(attr => attr.ToString());
                var model = compilation.GetSemanticModel(tree);
                var typeSymbol = model.GetDeclaredSymbol(typeDeclarationSyntax)!;
                var methods = GetMethodInfos(typeDeclarationSyntax, typeSymbol);

                var typeInfo = new TypeInfo(@namespace, typeName, relativeDirectory);
                typeInfo.Usings.AddRange(usings);
                typeInfo.Attributes.AddRange(attributes);
                typeInfo.Methods.AddRange(methods);

                context.Output = typeInfo;
                if (OutputVariableName is not null)
                {
                    context.SetVariable(OutputVariableName, typeInfo);
                }

                LogOutput(() => typeInfo);

                return Done();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Parsing {OutputVariableName ?? "???"} failed.");
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