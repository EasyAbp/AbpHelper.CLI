{{-
if Option.SkipCustomRepository
    if EntityInfo.CompositeKeyName
        repositoryType = "IRepository<" + EntityInfo.Name + ">"
    else
        repositoryType = "IRepository<" + EntityInfo.Name + ", " + EntityInfo.PrimaryKey + ">"
    end
    repositoryName = "Repository"
else
    repositoryType = "I" + EntityInfo.Name + "Repository"
    repositoryName = "_repository"
end ~}}
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;
{{~ if !EntityInfo.CompositeKeyName
    crudClassName = "CrudAppService"
else
    crudClassName = "AbstractKeyCrudAppService"
~}}
using System.Linq;
{{~ end -}}
{{~ if !Option.SkipPermissions 
    permissionNamesPrefix = ProjectInfo.Name + "Permissions." + EntityInfo.Name
~}}
using {{ ProjectInfo.FullName }}.Permissions;
{{~ end ~}}
using {{ EntityInfo.Namespace }}.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
{{~ if Option.SkipCustomRepository ~}}
using Volo.Abp.Domain.Repositories;
{{~ end ~}}

namespace {{ EntityInfo.Namespace }}
{
    public class {{ EntityInfo.Name }}AppService : {{ crudClassName }}<{{ EntityInfo.Name }}, {{ DtoInfo.ReadTypeName }}, {{ EntityInfo.PrimaryKey ?? EntityInfo.CompositeKeyName }}, PagedAndSortedResultRequestDto, {{ DtoInfo.CreateTypeName }}, {{ DtoInfo.UpdateTypeName }}>,
        I{{ EntityInfo.Name }}AppService
    {
        {{~ if !Option.SkipPermissions ~}}
        protected override string GetPolicyName { get; set; } = {{ permissionNamesPrefix }}.Default;
        protected override string GetListPolicyName { get; set; } = {{ permissionNamesPrefix }}.Default;
        protected override string CreatePolicyName { get; set; } = {{ permissionNamesPrefix }}.Create;
        protected override string UpdatePolicyName { get; set; } = {{ permissionNamesPrefix }}.Update;
        protected override string DeletePolicyName { get; set; } = {{ permissionNamesPrefix }}.Delete;
        {{~ end ~}}

        {{~ if !Option.SkipCustomRepository ~}}
        private readonly {{ repositoryType }} {{ repositoryName }};
        
        public {{ EntityInfo.Name }}AppService({{ repositoryType }} repository) : base(repository)
        {
            {{ repositoryName }} = repository;
        }
        {{~ else ~}}
        public {{ EntityInfo.Name }}AppService({{ repositoryType }} repository) : base(repository)
        {
        }
        {{~ end ~}}
        {{~ if EntityInfo.CompositeKeyName ~}}

        protected override Task DeleteByIdAsync({{ EntityInfo.CompositeKeyName }} id)
        {
            // TODO: AbpHelper generated
            return {{ repositoryName }}.DeleteAsync(e =>
            {{~ for prop in EntityInfo.CompositeKeys ~}}
                e.{{ prop.Name }} == id.{{ prop.Name}}{{ if !for.last}} &&{{end}}
            {{~ end ~}}
            );
        }

        protected override async Task<{{ EntityInfo.Name }}> GetEntityByIdAsync({{ EntityInfo.CompositeKeyName }} id)
        {
            // TODO: AbpHelper generated
            return await AsyncExecuter.FirstOrDefaultAsync(
                (await {{ repositoryName }}.WithDetailsAsync()).Where(e =>
                {{~ for prop in EntityInfo.CompositeKeys ~}}
                    e.{{ prop.Name }} == id.{{ prop.Name}}{{ if !for.last}} &&{{end}}
                {{~ end ~}}
                )
            ); 
        }

        protected override IQueryable<{{ EntityInfo.Name }}> ApplyDefaultSorting(IQueryable<{{ EntityInfo.Name }}> query)
        {
            // TODO: AbpHelper generated
            return query.OrderBy(e => e.{{ EntityInfo.CompositeKeys[0].Name }});
        }
        {{~ end ~}} 

        public async Task<PagedResultDto<{{ EntityInfo.Name }}Dto>> GetListByFilterAsync({{ DtoInfo.GetTypeName }} input)
        {
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = nameof({{ EntityInfo.Name }}.Name);
            }

            var {{ EntityInfo.Name }}s = await _repository.GetListAsync(
                input.SkipCount,
                input.MaxResultCount,
                input.Sorting,
                input.Filter
            );

            var totalCount = input.Filter == null
                ? await _repository.CountAsync()
                : await _repository.CountAsync(e => e.Name.Contains(input.Filter));

            return new PagedResultDto<{{ EntityInfo.Name }}Dto>(
                totalCount,
                ObjectMapper.Map<List<{{ EntityInfo.Name }}>, List<{{ EntityInfo.Name }}Dto>>({{ EntityInfo.Name }}s)
            );
        }
    }
}
