{{- if Option.SeparateDto
createDto = "Create" + EntityInfo.Name + "Dto"
updateDto = "Update" + EntityInfo.Name + "Dto"
else
createDto = "CreateUpdate" + EntityInfo.Name + "Dto"
updateDto = "CreateUpdate" + EntityInfo.Name + "Dto"
end
-}}
using System;
using {{ EntityInfo.Namespace }}.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace {{ EntityInfo.Namespace }}
{
    public interface I{{ EntityInfo.Name }}AppService :
        ICrudAppService< 
            {{ EntityInfo.Name }}Dto, 
            {{ EntityInfo.PrimaryKey ?? EntityInfo.CompositeKeyName }}, 
            PagedAndSortedResultRequestDto,
            {{ createDto }},
            {{ updateDto }}>
    {

    }
}