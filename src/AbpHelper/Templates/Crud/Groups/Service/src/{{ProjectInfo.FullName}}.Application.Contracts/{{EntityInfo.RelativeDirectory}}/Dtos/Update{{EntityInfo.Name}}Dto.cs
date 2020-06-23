{{- SKIP_GENERATE = !Option.SeparateDto -}}
using System;
using System.ComponentModel;

namespace {{ EntityInfo.Namespace }}.Dtos
{
    [Serializable]
    public class Update{{ EntityInfo.Name }}Dto
    {
        {{~ for prop in EntityInfo.Properties ~}}
        {{~ if prop | abp.is_ignore_property; continue; end ~}}
        {{~ if !Option.SkipLocalization && Option.SkipViewModel ~}}
        [DisplayName("{{ EntityInfo.Name + prop.Name}}")]
        {{~ end ~}}
        {{~ if string.ends_with prop.Type ">" ~}}
        public {{ prop.Type | string.replace ">" "Dto>"}} {{ prop.Name }}Dto { get; set; }
        {{~ else ~}}
        public {{ prop.Type}} {{ prop.Name }} { get; set; }
        {{~end ~}}
        {{~ if !for.last ~}}

        {{~ end ~}}
        {{~ end ~}}
    }
}