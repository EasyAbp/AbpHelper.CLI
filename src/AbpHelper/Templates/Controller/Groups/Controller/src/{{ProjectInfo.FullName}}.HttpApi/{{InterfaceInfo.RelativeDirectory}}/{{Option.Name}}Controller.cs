{{~ for using in InterfaceInfo.Usings ~}}
{{~ if using != "Volo.Abp.Application.Services" ~}}
using {{ using }};
{{~ end ~}}
{{~ end ~}}
{{~ if ProjectInfo.TemplateType == 'Application' ~}}
using {{ ProjectInfo.FullName }}.Controllers;
{{~ end ~}}
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace {{ ProjectInfo.FullName }}.{{ InterfaceInfo.RelativeNamespace }}
{
    {{~ for attribute in InterfaceInfo.Attributes}}
    attribute
    {{~ end ~}}
    [RemoteService(Name = "{{ ProjectInfo.Name }}{{ Option.Name }}")]
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
        {{~ for method in ClassInfo.Methods | abp.intersect InterfaceInfo.Methods ~}}
{{~ include "Templates/Controller/ControllerMethod" method ~}}
        {{~ end ~}}
    }
}