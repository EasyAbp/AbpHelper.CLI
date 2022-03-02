using System;
using System.Threading.Tasks;
using {{ EntityInfo.Namespace }}.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace {{ EntityInfo.Namespace }}
{
    public interface I{{ EntityInfo.Name }}AppService :
        ICrudAppService< 
            {{ DtoInfo.ReadTypeName }}, 
            {{ EntityInfo.PrimaryKey ?? EntityInfo.CompositeKeyName }}, 
            PagedAndSortedResultRequestDto,
            {{ DtoInfo.CreateTypeName }},
            {{ DtoInfo.UpdateTypeName }}>
    {
        Task<PagedResultDto<{{ EntityInfo.Name }}Dto>> GetListByFilterAsync({{ DtoInfo.GetTypeName }} input);
    }
}