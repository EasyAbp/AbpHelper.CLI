using System;
using System.Threading.Tasks;
using {{ ProjectInfo.FullName }}.Controllers;
using {{ ProjectInfo.FullName }}.{{ ServiceInfo.RelativeDirectory }}.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace {{ ProjectInfo.FullName }}.{{ ServiceInfo.RelativeDirectory }}
{
    [RemoteService(Name = "{{ Option.Name }}Service")]
    [Route("/api/{{ Option.Name | abp.camel_case }}")]
    public class {{ Option.Name }}Controller : {{ ProjectInfo.Name }}Controller, I{{ Option.Name }}AppService
    {
        private readonly I{{ Option.Name }}AppService _service;

        public {{ Option.Name }}Controller(I{{ Option.Name }}AppService service)
        {
            _service = service;
        }

        {{~ for method in ServiceInfo.Methods ~}}
        {{ method.Accessor }} {{ method.ReturnType }} {{  method.Name }}({{ for param in method.Parameters }}{{ param.Type }} {{ param.Name }}{{if !for.last }}, {{ end }}{{ end }})
        {
            {{ if method.ReturnType != "void" }}return {{ end }}_service.{{ method.Name }}({{ for param in method.Parameters }}{{ param.Name }}{{if !for.last }}, {{ end }}{{ end }});
        }
        {{~ if !for.last ~}}

        {{~ end ~}}
        {{~ end ~}}
    }
}