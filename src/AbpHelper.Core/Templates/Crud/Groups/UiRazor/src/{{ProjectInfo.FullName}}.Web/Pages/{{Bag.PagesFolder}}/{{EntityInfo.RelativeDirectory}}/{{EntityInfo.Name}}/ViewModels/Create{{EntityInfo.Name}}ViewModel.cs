{{- SKIP_GENERATE = !Option.SeparateDto || Option.SkipViewModel -}}
using System;
{{~ if !Option.SkipLocalization ~}}
using System.ComponentModel.DataAnnotations;
{{~ end ~}}
{{~ if Bag.PagesFolder; pagesNamespace = Bag.PagesFolder + "."; end ~}}

namespace {{ ProjectInfo.FullName }}.Web.Pages.{{ pagesNamespace }}{{ EntityInfo.RelativeNamespace}}.{{ EntityInfo.Name }}.ViewModels;

public class Create{{ EntityInfo.Name }}ViewModel
{
    {{~ for prop in EntityInfo.Properties ~}}
    {{~ if prop | abp.is_ignore_property; continue; end ~}}
    {{~ if !Option.SkipLocalization ~}}
    [Display(Name = "{{ EntityInfo.Name + prop.Name}}")]
    {{~ end ~}}
    public {{ prop.Type}} {{ prop.Name }} { get; set; }
    {{~ if !for.last ~}}

    {{~ end ~}}
    {{~ end ~}}
}
