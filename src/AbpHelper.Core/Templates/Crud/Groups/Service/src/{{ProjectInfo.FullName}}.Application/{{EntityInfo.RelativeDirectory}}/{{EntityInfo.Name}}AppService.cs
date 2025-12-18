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
end ~}}
{{~ if EntityInfo.CompositeKeyName || !Option.SkipGetListInputDto~}}
using System.Linq;
using System.Threading.Tasks;
{{~ end -}}
{{~ if !Option.SkipPermissions
    permissionNamesPrefix = ProjectInfo.Name + "Permissions." + EntityInfo.Name
~}}
using {{ ProjectInfo.FullName }}.Permissions;
{{~ end ~}}
using {{ EntityInfo.Namespace }}.Dtos;
{{~ if Option.SkipGetListInputDto ~}}
using Volo.Abp.Application.Dtos;
{{~ end ~}}
using Volo.Abp.Application.Services;
{{~ if Option.SkipCustomRepository ~}}
using Volo.Abp.Domain.Repositories;
{{~ end ~}}

namespace {{ EntityInfo.Namespace }};

{{~
    if !Option.SkipGetListInputDto
        TGetListInput = EntityInfo.Name + "GetListInput"
    else
        TGetListInput = "PagedAndSortedResultRequestDto"
end ~}}

{{~ if EntityInfo.Document | !string.whitespace ~}}
/// <summary>
/// {{ EntityInfo.Document }}
/// </summary>
{{~ end ~}}
public class {{ EntityInfo.Name }}AppService : {{ crudClassName }}<{{ EntityInfo.Name }}, {{ DtoInfo.ReadTypeName }}, {{ EntityInfo.PrimaryKey ?? EntityInfo.CompositeKeyName }}, {{TGetListInput}}, {{ DtoInfo.CreateTypeName }}, {{ DtoInfo.UpdateTypeName }}>,
    I{{ EntityInfo.Name }}AppService
{
    {{~ if !Option.SkipPermissions ~}}
    protected override string? GetPolicyName { get; set; } = {{ permissionNamesPrefix }}.Default;
    protected override string? GetListPolicyName { get; set; } = {{ permissionNamesPrefix }}.Default;
    protected override string? CreatePolicyName { get; set; } = {{ permissionNamesPrefix }}.Create;
    protected override string? UpdatePolicyName { get; set; } = {{ permissionNamesPrefix }}.Update;
    protected override string? DeletePolicyName { get; set; } = {{ permissionNamesPrefix }}.Delete;
    {{~ end ~}}

    {{~ if !Option.SkipCustomRepository ~}}
    private readonly {{ repositoryType }} {{ repositoryName }};

    public {{ EntityInfo.Name }}AppService({{ repositoryType }} repository) : base(repository)
    {
        {{ repositoryName }} = repository;

        LocalizationResource = typeof({{ ProjectInfo.Name }}Resource);
        ObjectMapperContext = typeof({{ ProjectInfo.Name }}ApplicationModule);
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
            ));
    }

    protected override IQueryable<{{ EntityInfo.Name }}> ApplyDefaultSorting(IQueryable<{{ EntityInfo.Name }}> query)
    {
        // TODO: AbpHelper generated
        return query.OrderBy(e => e.{{ EntityInfo.CompositeKeys[0].Name }});
    }
    {{~ end ~}}

    {{~ if !Option.SkipGetListInputDto ~}}
    protected override async Task<IQueryable<{{ EntityInfo.Name }}>> CreateFilteredQueryAsync({{ EntityInfo.Name }}GetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            {{~ for prop in EntityInfo.Properties ~}}
            {{~ if (prop | abp.is_ignore_property) || string.starts_with prop.Type "List<"; continue; end ~}}
            {{~ if prop.Type == "string" ~}}
            .WhereIf(!input.{{ prop.Name }}.IsNullOrWhiteSpace(), x => x.{{ prop.Name }}.Contains(input.{{ prop.Name }}))
            {{~ else ~}}
            .WhereIf(input.{{ prop.Name }} != null, x => x.{{ prop.Name }} == input.{{ prop.Name }})
            {{~ end ~}}
            {{~ end ~}}
            ;
    }
    {{~ end ~}}
}
