{{~ for using in InterfaceInfo.Usings ~}}
{{~ if using != "Volo.Abp.Application.Services" && using != "System.Threading.Tasks" ~}}
using {{ using }};
{{~ end ~}}
{{~ end ~}}
using System.Threading.Tasks;
{{~ if ProjectInfo.TemplateType == 'Application' ~}}
using {{ ProjectInfo.FullName }}.Controllers;
{{~ end ~}}
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace {{ ProjectInfo.FullName }}.{{ InterfaceInfo.RelativeNamespace }};

{{~ if ProjectInfo.TemplateType == 'Application' ~}}
[RemoteService(Name = "Default")]
{{~ else if ProjectInfo.TemplateType == 'Module' ~}}
[RemoteService(Name = {{ ProjectInfo.Name }}RemoteServiceConsts.RemoteServiceName)]
{{~ end ~}}
{{~ if ProjectInfo.TemplateType == 'Application' ~}}
[Route("/api/app/{{ Option.Name | abp.kebab_case }}")]
{{~ else if ProjectInfo.TemplateType == 'Module' ~}}
[Route("/api/{{ ProjectInfo.Name | abp.kebab_case }}/{{ Option.Name | abp.kebab_case }}")]
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