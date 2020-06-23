using System;
using Volo.Abp.Application.Dtos;

namespace {{ EntityInfo.Namespace }}.Dtos
{
    [Serializable]
    public class {{ EntityInfo.Name }}Dto : {{ EntityInfo.BaseType | string.replace "AggregateRoot" "Entity"}}Dto{{ if EntityInfo.PrimaryKey }}<{{ EntityInfo.PrimaryKey}}>{{ end }}
    {
        {{~ for prop in EntityInfo.Properties ~}}
        {{~ if prop | abp.is_ignore_property; continue; end ~}}
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