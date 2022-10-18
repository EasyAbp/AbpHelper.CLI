using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;

namespace EasyAbp.AbpHelper.Core.Steps.Abp
{
    [Activity(
        Category = "SetModelVariableStep",
        Description = "SetModelVariableStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class SetModelVariableStep : Step
    {
        [ActivityInput(
            Hint = "ProjectInfo",
            UIHint = ActivityInputUIHints.SingleLine,
            DefaultSyntax = SyntaxNames.JavaScript,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public ProjectInfo? ProjectInfo
        {
            get => GetState<ProjectInfo?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "Option",
            UIHint = ActivityInputUIHints.SingleLine,
            DefaultSyntax = SyntaxNames.JavaScript,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public object? Option
        {
            get => GetState<object?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "EntityInfo",
            UIHint = ActivityInputUIHints.SingleLine,
            DefaultSyntax = SyntaxNames.JavaScript,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public EntityInfo? EntityInfo
        {
            get => GetState<EntityInfo?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "InterfaceInfo",
            UIHint = ActivityInputUIHints.SingleLine,
            DefaultSyntax = SyntaxNames.JavaScript,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public TypeInfo? InterfaceInfo
        {
            get => GetState<TypeInfo?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "ClassInfo",
            UIHint = ActivityInputUIHints.SingleLine,
            DefaultSyntax = SyntaxNames.JavaScript,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public TypeInfo? ClassInfo
        {
            get => GetState<TypeInfo?>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "DtoInfo",
            UIHint = ActivityInputUIHints.SingleLine,
            DefaultSyntax = SyntaxNames.JavaScript,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public DtoInfo? DtoInfo
        {
            get => GetState<DtoInfo?>();
            set => SetState(value);
        }

        protected override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            ProjectInfo ??= context.GetVariable<ProjectInfo?>("ProjectInfo");
            Option ??= context.GetVariable<object?>("Option");
            EntityInfo ??= context.GetVariable<EntityInfo?>("EntityInfo");
            InterfaceInfo ??= context.GetVariable<TypeInfo?>("InterfaceInfo");
            ClassInfo ??= context.GetVariable<TypeInfo?>("ClassInfo");
            DtoInfo ??= context.GetVariable<DtoInfo?>("DtoInfo");

            var variables = context.WorkflowExecutionContext.GetMergedVariables().Data
                .Where(v => v.Key.StartsWith("Bag."));
            var bag = new ExpandoObject();

            foreach (var variable in variables)
            {
                ((IDictionary<string, object?>)bag)[variable.Key.RemovePreFix("Bag.")] = variable.Value;
            }

            context.SetVariable("Model", new
            {
                ProjectInfo,
                Option,
                EntityInfo,
                InterfaceInfo,
                ClassInfo,
                Bag = bag,
                DtoInfo,
            });

            return new ValueTask<IActivityExecutionResult>(Done());
        }
    }
}