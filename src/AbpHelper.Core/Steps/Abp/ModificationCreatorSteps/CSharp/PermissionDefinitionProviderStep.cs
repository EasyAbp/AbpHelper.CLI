using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Extensions;
using EasyAbp.AbpHelper.Core.Generator;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Core.Steps.Abp.ModificationCreatorSteps.CSharp
{
    [Activity(
        Category = "PermissionDefinitionProviderStep",
        Description = "PermissionDefinitionProviderStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class PermissionDefinitionProviderStep : CSharpModificationCreatorStep
    {
        [ActivityInput(
            Hint = "ProjectInfo",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public ProjectInfo? ProjectInfo
        {
            get => GetState<ProjectInfo?>();
            set => SetState(value);
        }

        protected override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            ProjectInfo ??= context.GetVariable<ProjectInfo>("ProjectInfo")!;

            LogInput(() => ProjectInfo);

            return base.OnExecuteAsync(context);
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var groupName = rootUnit.Descendants<LocalDeclarationStatementSyntax>()
                .Single(stat => stat.ToFullString().Contains("context.AddGroup"))
                .Descendants<VariableDeclarationSyntax>().Single()
                .Variables[0].Identifier.Text;
            var model = context.GetVariable<object>("Model")! as dynamic;
            model.Bag.GroupName = groupName;
            string permissionDefinitionsText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "Permissions_Definitions", model);

            var builders = new List<ModificationBuilder<CSharpSyntaxNode>>();

            builders.Add(new InsertionBuilder<CSharpSyntaxNode>(
                root => root.Descendants<MethodDeclarationSyntax>().First().GetEndLine(),
                permissionDefinitionsText,
                InsertPosition.Before,
                root => root.DescendantsNotContain<ClassDeclarationSyntax>(permissionDefinitionsText)
            ));

            if (ProjectInfo.TemplateType == TemplateType.Application)
            {
                // Noting special to do
            }
            else if (ProjectInfo.TemplateType == TemplateType.Module)
            {
                string addGroupText =
                    TextGenerator.GenerateByTemplateName(TemplateDirectory, "Permissions_AddGroup", model);

                // Uncomment the add group statement
                builders.Add(new ReplacementBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<MethodDeclarationSyntax>().First().GetStartLine() + 2,
                    root => root.Descendants<MethodDeclarationSyntax>().First().GetStartLine() + 2,
                    addGroupText,
                    modifyCondition: root => !root.DescendantsNotContain<MethodDeclarationSyntax>($"//" + addGroupText)
                ));
            }

            return builders;
        }

        public PermissionDefinitionProviderStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}