using System;
using Volo.Abp.Application.Dtos;

namespace {{ EntityInfo.Namespace }}.Dtos
{
    [Serializable]
    public class {{ DtoInfo.ReadTypeName }} : {{ EntityInfo.BaseType | string.replace "AggregateRoot" "Entity"}}Dto{{ if EntityInfo.PrimaryKey }}<{{ EntityInfo.PrimaryKey}}>{{ end }}
    {
        {{~ for prop in EntityInfo.Properties ~}}
        {{~ if prop | abp.is_ignore_property; continue; end ~}}
        public {{ prop.Type}} {{ prop.Name }} { get; set; }
        {{~ if !for.last ~}}

        {{~ end ~}}
        {{~ end ~}}
    }
}