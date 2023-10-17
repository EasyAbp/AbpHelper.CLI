{{- SKIP_GENERATE = EntityInfo.CompositeKeyName == null -}}
using System;

namespace {{ EntityInfo.Namespace }}.Dtos;

{{~ if EntityInfo.Document | !string.whitespace ~}}
/// <summary>
/// {{ EntityInfo.Document }}
/// </summary>
{{~ end ~}}
public class {{ EntityInfo.CompositeKeyName }}
{
    {{~ for prop in EntityInfo.CompositeKeys ~}}
    {{~ if prop.Document| !string.whitespace ~}}
    /// <summary>
    /// {{ prop.Document }}
    /// </summary>
    {{~ end ~}} 
    public {{ prop.Type}} {{ prop.Name }} { get; set; }
    {{~ if !for.last ~}}

    {{~ end ~}}
    {{~ end ~}}
}