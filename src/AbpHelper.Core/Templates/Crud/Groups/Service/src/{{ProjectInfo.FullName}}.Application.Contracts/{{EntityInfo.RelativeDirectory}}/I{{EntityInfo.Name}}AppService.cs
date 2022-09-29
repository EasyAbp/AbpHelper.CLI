using System;
using {{ EntityInfo.Namespace }}.Dtos;
{{~ if !Option.ExtensionPagedAndSortedResultRequestDto ~}}
using Volo.Abp.Application.Dtos;
{{~ end ~}}
using Volo.Abp.Application.Services;

namespace {{ EntityInfo.Namespace }};

{{
    if Option.ExtensionPagedAndSortedResultRequestDto
        TGetListInput = EntityInfo.Name + "GetListInput"
    else
        TGetListInput = "PagedAndSortedResultRequestDto"
end ~}}
public interface I{{ EntityInfo.Name }}AppService :
    ICrudAppService< 
        {{ DtoInfo.ReadTypeName }}, 
        {{ EntityInfo.PrimaryKey ?? EntityInfo.CompositeKeyName }}, 
        {{TGetListInput}},
        {{ DtoInfo.CreateTypeName }},
        {{ DtoInfo.UpdateTypeName }}>
{

}