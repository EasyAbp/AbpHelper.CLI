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
        Category = "MenuContributorStep",
        Description = "MenuContributorStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class MenuContributorStep : CSharpModificationCreatorStep
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

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            ProjectInfo ??= context.GetVariable<ProjectInfo>("ProjectInfo")!;

            LogInput(() => ProjectInfo);

            return await base.OnExecuteAsync(context);
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(
            ActivityExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var model = context.GetVariable<object>("Model")!;
            
            var addMenuItemText =
                TextGenerator.GenerateByTemplateName(TemplateDirectory, "MenuContributor_AddMenuItem", model);

            CSharpSyntaxNode MainMenu(CSharpSyntaxNode root) => root.Descendants<MethodDeclarationSyntax>()
                .First(n => n.Identifier.ToString().Contains("ConfigureMainMenu"));

            var builders = new List<ModificationBuilder<CSharpSyntaxNode>>();

            if (ProjectInfo.TemplateType == TemplateType.Application)
            {
                var usingForAppText =
                    TextGenerator.GenerateByTemplateName(TemplateDirectory, "MenuContributor_UsingForApp", model);

                builders.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => 2,
                        usingForAppText,
                        modifyCondition: root => root.NotContains(usingForAppText)
                    ));
                builders.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => MainMenu(root).GetEndLine(),
                        addMenuItemText,
                        modifyCondition: root => MainMenu(root).NotContains(addMenuItemText)
                    )
                );
            }
            else if (ProjectInfo.TemplateType == TemplateType.Module)
            {
                var configureMainMenuText =
                    TextGenerator.GenerateByTemplateName(TemplateDirectory, "MenuContributor_ConfigureMainMenu", model);
                var usingForModuleText =
                    TextGenerator.GenerateByTemplateName(TemplateDirectory, "MenuContributor_UsingForModule", model);
                var localizerText =
                    TextGenerator.GenerateByTemplateName(TemplateDirectory, "MenuContributor_Localizer", model);
                builders.Add(
                    new ReplacementBuilder<CSharpSyntaxNode>(
                        root => MainMenu(root).GetStartLine(),
                        root => MainMenu(root).GetStartLine(),
                        configureMainMenuText,
                        modifyCondition: root => root.NotContains(configureMainMenuText)
                    ));
                builders.Add(
                    new InsertionBuilder<CSharpSyntaxNode>(
                        root => 2,
                        usingForModuleText,
                        modifyCondition: root => root.NotContains(usingForModuleText)
                    ));
                builders.Add(new InsertionBuilder<CSharpSyntaxNode>(
                    root => MainMenu(root).GetStartLine() + 2,
                    localizerText,
                    modifyCondition: root => MainMenu(root).NotContains(localizerText)
                ));
                builders.Add(new DeletionBuilder<CSharpSyntaxNode>(
                    root => MainMenu(root).GetEndLine() - 1,
                    root => MainMenu(root).GetEndLine() - 1,
                    modifyCondition: root => !root.NotContains("return Task.CompletedTask;")
                ));
                builders.Add(new InsertionBuilder<CSharpSyntaxNode>(
                    root => MainMenu(root).GetEndLine(),
                    addMenuItemText,
                    modifyCondition: root => MainMenu(root).NotContains(addMenuItemText)
                ));
            }

            return builders;
        }

        public MenuContributorStep(TextGenerator textGenerator) : base(textGenerator)
        {
        }
    }
}