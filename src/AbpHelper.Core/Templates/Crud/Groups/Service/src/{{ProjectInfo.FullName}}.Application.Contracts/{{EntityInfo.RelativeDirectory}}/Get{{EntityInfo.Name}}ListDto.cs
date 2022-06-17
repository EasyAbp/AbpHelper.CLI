using System;
using Volo.Abp.Application.Dtos;

namespace {{ EntityInfo.Namespace }}
{
    public class Get{{EntityInfo.Name}}ListDto : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }    
}