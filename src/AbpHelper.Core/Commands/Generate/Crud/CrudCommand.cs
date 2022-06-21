using System;
using System.Linq;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using EasyAbp.AbpHelper.Core.Workflow;
using EasyAbp.AbpHelper.Core.Workflow.Generate;
using EasyAbp.AbpHelper.Core.Workflow.Generate.Crud;
using Elsa;
using Elsa.Activities.ControlFlow;
using Elsa.Builders;
using Elsa.Activities.Primitives;
using IActivityBuilder = Elsa.Builders.IActivityBuilder;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Crud
{
    public class CrudCommand : CommandWithOption<CrudCommandOption>
    {
        private const string DbMigrations = "DbMigrations";
        private const string TestGeneration = "TestGeneration";

        public CrudCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "crud", "Generate a set of CRUD related files according to the specified entity")
        {
        }

        protected override IActivityBuilder ConfigureBuild(CrudCommandOption option, IActivityBuilder activityBuilder)
        {
            var entityFileName = option.Entity + ".cs";

            return base.ConfigureBuild(option, activityBuilder)
                .AddOverwriteWorkflow(option)
                .Then<SetVariable>(step =>
                {
                    step.Set(x => x.VariableName, VariableNames.TemplateDirectory);
                    step.Set(x => x.Value, "/Templates/Crud");
                })
                .Then<FileFinderStep>(
                    step => { step.Set(x => x.SearchFileName, entityFileName); })
                .Then<EntityParserStep>()
                .Then<BuildDtoInfoStep>(step =>
                {
                    step.Set(x => x.EntityInfo, x => x.GetVariable<EntityInfo>("EntityInfo"));
                    step.Set(x => x.DtoSuffix, option.DtoSuffix);
                    step.Set(x => x.SeparateDto, option.SeparateDto);
                    step.Set(x => x.EntityPrefixDto, option.EntityPrefixDto);
                })
                .Then<SetModelVariableStep>()
                .Then<If>(
                    step => { step.Set(x => x.Condition, option.SkipEntityConstructors); },
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.False)
                            .AddEntityConstructorsGenerationWorkflow()
                            .ThenNamed("EntityUsing")
                            ;
                        ifElse.When(OutcomeNames.True)
                            .ThenNamed("EntityUsing")
                            ;
                    })
                .AddEntityUsingGenerationWorkflow("EntityUsing")
                .AddEfCoreConfigurationWorkflow()
                .Then<If>(
                    step => { step.Set(x => x.Condition, option.SkipCustomRepository); },
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .AddCustomRepositoryGeneration()
                            .ThenNamed("ServiceGeneration")
                            ;
                        ifElse
                            .When(OutcomeNames.True)
                            .ThenNamed("ServiceGeneration")
                            ;
                    }
                )
                .AddServiceGenerationWorkflow("ServiceGeneration", option)
                .Then<If>(
                    step => { step.Set(x => x.Condition, option.SkipLocalization); },
                    ifElse =>
                    {
                        ifElse.When(OutcomeNames.False)
                            .AddLocalizationGenerationWorkflow()
                            .ThenNamed(ActivityNames.Ui)
                            ;
                        ifElse.When(OutcomeNames.True)
                            .ThenNamed(ActivityNames.Ui)
                            ;
                    })
                .Then<If>(
                    step => { step.Set(x => x.Condition, option.SkipUi); },
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .Then<Switch>(
                                step =>
                                {
                                    step.WithCases(context =>
                                    {
                                        var projectInfo = context.GetVariable<ProjectInfo>("ProjectInfo")!;
                                        return Enum.GetValues<UiFramework>().Select(u =>
                                            new SwitchCase(u.ToString(), projectInfo.UiFramework == u)).ToList();
                                    });
                                },
                                @switch =>
                                {
                                    @switch.When(UiFramework.None.ToString("D"))
                                        .ThenNamed(TestGeneration);

                                    @switch.When(UiFramework.RazorPages.ToString("D"))
                                        .AddUiRazorPagesGenerationWorkflow()
                                        .ThenNamed(TestGeneration);

                                    @switch.When(UiFramework.Angular.ToString("D"))
                                        // TODO
                                        //.AddUiAngularGenerationWorkflow()
                                        .ThenNamed(TestGeneration);
                                }
                            )
                            ;
                        ifElse
                            .When(OutcomeNames.True)
                            .ThenNamed(TestGeneration)
                            ;
                    }
                ).WithName(ActivityNames.Ui)
                .Then<If>(
                    step => { step.Set(x => x.Condition, option.SkipTest); },
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .AddTestGenerationWorkflow()
                            .ThenNamed(DbMigrations)
                            ;
                        ifElse
                            .When(OutcomeNames.True)
                            .ThenNamed(DbMigrations)
                            ;
                    }
                ).WithName(TestGeneration)
                .Then<If>(
                    step => { step.Set(x => x.Condition, option.SkipDbMigrations); },
                    ifElse =>
                    {
                        ifElse
                            .When(OutcomeNames.False)
                            .AddMigrationAndUpdateDatabaseWorkflow()
                            ;
                    }
                ).WithName(DbMigrations);
        }
    }
}