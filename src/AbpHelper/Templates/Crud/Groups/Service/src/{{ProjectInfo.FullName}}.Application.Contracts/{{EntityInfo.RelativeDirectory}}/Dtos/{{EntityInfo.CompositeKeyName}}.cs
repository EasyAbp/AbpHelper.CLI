{{- SKIP_GENERATE = EntityInfo.CompositeKeyName == null -}}
namespace {{ EntityInfo.Namespace }}.Dtos
{
    public class {{ EntityInfo.CompositeKeyName }}
    {
        {{~ for prop in EntityInfo.CompositeKeys ~}}
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