using System;
using Volo.Abp.Application.Dtos;

namespace {{ EntityInfo.Namespace }}
{
    public class {{ DtoInfo.GetTypeName }} : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }    
}