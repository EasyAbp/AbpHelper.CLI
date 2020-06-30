using System;
using System.Threading.Tasks;
{{~ if ProjectInfo.TemplateType == 'Application' ~}}
using {{ ProjectInfo.FullName }}.Controllers;
{{~ end ~}}
using {{ ProjectInfo.FullName }}.{{ ServiceInfo.RelativeDirectory }}.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace {{ ProjectInfo.FullName }}.{{ ServiceInfo.RelativeDirectory }}
{
    [RemoteService(Name = "{{ Option.Name }}Service")]
    {{~ if ProjectInfo.TemplateType == 'Application' ~}}
    [Route("/api/app/{{ Option.Name | abp.camel_case }}")]
    {{~ else if ProjectInfo.TemplateType == 'Module' ~}}
    [Route("/api/{{ ProjectInfo.Name | abp.camel_case }}/{{ Option.Name | abp.camel_case }}")]
    {{~ end ~}}
    public class {{ Option.Name }}Controller : {{ ProjectInfo.Name }}Controller, I{{ Option.Name }}AppService
    {
        private readonly I{{ Option.Name }}AppService _service;

        public {{ Option.Name }}Controller(I{{ Option.Name }}AppService service)
        {
            _service = service;
        }
        {{~ for method in ServiceInfo.Methods ~}}
{{~ include "Templates/Controller/ControllerMethod" method ~}}
        {{~ end ~}}
    }
}