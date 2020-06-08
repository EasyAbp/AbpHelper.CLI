{{- SKIP_GENERATE = EntityInfo.CompositeKeyName == null -}}
namespace {{ EntityInfo.Namespace }}.Dtos
{
    public class {{ EntityInfo.CompositeKeyName }}
    {
        {{~ for prop in EntityInfo.CompositeKeys ~}}
        public {{ prop.Type}} {{ prop.Name }} { get; set; }
        {{~ if !for.last ~}}

        {{~ end ~}}
        {{~ end ~}}
    }
}