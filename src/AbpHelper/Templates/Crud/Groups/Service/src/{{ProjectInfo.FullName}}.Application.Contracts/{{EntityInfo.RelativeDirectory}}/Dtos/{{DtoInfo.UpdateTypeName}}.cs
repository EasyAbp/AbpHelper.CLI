{{- SKIP_GENERATE = DtoInfo.CreateTypeName == DtoInfo.UpdateTypeName -}}
using System;
using System.ComponentModel;

namespace {{ EntityInfo.Namespace }}.Dtos
{
    [Serializable]
    public class {{ DtoInfo.UpdateTypeName }}
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