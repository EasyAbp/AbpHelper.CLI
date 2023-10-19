{{- SKIP_GENERATE = !Option.IntegrationService -}}
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
[Route("/integration-api/app/{{ Option.Name | abp.kebab_case }}")]
{{~ else if ProjectInfo.TemplateType == 'Module' ~}}
[Route("/integration-api/{{ ProjectInfo.Name | abp.kebab_case }}/{{ Option.Name | abp.kebab_case }}")]
{{~ end ~}}
public class {{ Option.Name }}IntegrationController : {{ ProjectInfo.Name }}Controller, I{{ Option.Name }}IntegrationService
{
    private readonly I{{ Option.Name }}IntegrationService _service;

    public {{ Option.Name }}IntegrationController(I{{ Option.Name }}IntegrationService service)
    {
        _service = service;
    }
{{~ for method in ClassInfo.Methods | abp.intersect InterfaceInfo.Methods ~}}
{{~ include "Templates/Controller/ControllerMethod" method ~}}
{{~ end ~}}
}