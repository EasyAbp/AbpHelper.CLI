{{- SKIP_GENERATE = !Option.SeparateDto -}}
using System;
{{~ if !Option.SkipLocalization }}using System.ComponentModel;{{ end ~}}

namespace {{ EntityInfo.Namespace }}.Dtos
{
    [Serializable]
    public class Create{{ EntityInfo.Name }}Dto
    {
        {{~ for prop in EntityInfo.Properties ~}}
        {{~ if prop | abp.is_ignore_property; continue; end ~}}
        {{~ if !Option.SkipLocalization && Option.SkipViewModel ~}}
        [DisplayName("{{ EntityInfo.Name + prop.Name}}")]
        {{~ end ~}}
        public {{ prop.Type}} {{ prop.Name }} { get; set; }
        {{~ if !for.last ~}}

        {{~ end ~}}
        {{~ end ~}}
    }
}