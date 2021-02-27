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
{{~ if !EntityInfo.CompositeKeyName
    crudClassName = "CrudAppService"
else
    crudClassName = "AbstractKeyCrudAppService"
~}}
using System.Linq;
using System.Threading.Tasks;
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
                {{ repositoryName }}.Where(e =>
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
    }
}
