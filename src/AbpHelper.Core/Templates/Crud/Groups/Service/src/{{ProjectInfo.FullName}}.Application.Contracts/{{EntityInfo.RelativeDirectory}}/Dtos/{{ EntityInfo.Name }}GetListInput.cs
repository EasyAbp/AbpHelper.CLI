{{- SKIP_GENERATE = Option.SkipGetListInputDto -}}
using System;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace {{ EntityInfo.Namespace }}.Dtos;

[Serializable]
public class {{ EntityInfo.Name }}GetListInput : PagedAndSortedResultRequestDto
{
    {{~ for prop in EntityInfo.Properties ~}}
    {{~ if prop | abp.is_ignore_property; continue; end ~}}
    {{~ if !Option.SkipLocalization && Option.SkipViewModel ~}}
    [DisplayName("{{ EntityInfo.Name + prop.Name}}")]
    {{~ end ~}}
    public {{ prop.Type}}{{- if prop.Type!="string";"?";end}} {{ prop.Name }} { get; set; }
    {{~ end ~}}
}