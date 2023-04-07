{{- SKIP_GENERATE = EntityInfo.CompositeKeyName == null -}}
using System;

namespace {{ EntityInfo.Namespace }}.Dtos;

/// <summary>
/// {{ EntityInfo.Document }}
/// </summary>
public class {{ EntityInfo.CompositeKeyName }}
{
    {{~ for prop in EntityInfo.CompositeKeys ~}}
    /// <summary>
    /// {{ prop.Document }}
    /// </summary>
    public {{ prop.Type}} {{ prop.Name }} { get; set; }
    {{~ if !for.last ~}}

    {{~ end ~}}
    {{~ end ~}}
}