using System;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Models;
using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Core.Steps.Abp
{
    [Activity(
        Category = "BuildDtoInfoStep",
        Description = "BuildDtoInfoStep",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class BuildDtoInfoStep : Step
    {
        [ActivityInput(
            Hint = "EntityInfo",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public EntityInfo EntityInfo
        {
            get => GetState<EntityInfo>()!;
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "SeparateDto",
            UIHint = ActivityInputUIHints.Checkbox,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public bool SeparateDto
        {
            get => GetState<bool>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "EntityPrefixDto",
            UIHint = ActivityInputUIHints.Checkbox,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public bool EntityPrefixDto
        {
            get => GetState<bool>();
            set => SetState(value);
        }

        [ActivityInput(
            Hint = "DtoSuffix",
            UIHint = ActivityInputUIHints.SingleLine,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript }
        )]
        public string? DtoSuffix
        {
            get => GetState<string?>();
            set => SetState(value);
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            try
            {
                string[] actionNames = { string.Empty, string.Empty, string.Empty };

                if (SeparateDto)
                {
                    actionNames[1] = "Create";
                    actionNames[2] = "Update";
                }
                else
                {
                    actionNames[1] = "CreateUpdate";
                    actionNames[2] = actionNames[1];
                }

                var typeNames = new string[actionNames.Length];

                var dtoSubfix = DtoSuffix ?? "Dto";

                for (var i = 0; i < typeNames.Length; i++)
                {
                    typeNames[i] = EntityPrefixDto
                        ? $"{EntityInfo.Name}{actionNames[i]}{dtoSubfix}"
                        : $"{actionNames[i]}{EntityInfo.Name}{dtoSubfix}";
                }

                var dtoInfo = new DtoInfo(typeNames[0], typeNames[1], typeNames[2]);

                context.Output = dtoInfo;
                context.SetVariable("DtoInfo", dtoInfo);
                LogOutput(() => dtoInfo);

                return Done();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Building DTO info failed.");
                if (e is ParseException pe)
                    foreach (var error in pe.Errors)
                        Logger.LogError(error);
                throw;
            }
        }
    }
}