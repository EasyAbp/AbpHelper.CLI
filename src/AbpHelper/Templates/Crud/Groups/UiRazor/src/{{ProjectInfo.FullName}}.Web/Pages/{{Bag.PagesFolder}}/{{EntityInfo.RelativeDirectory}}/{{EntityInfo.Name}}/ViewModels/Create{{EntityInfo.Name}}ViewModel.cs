{{- SKIP_GENERATE = !Option.SeparateDto || Option.SkipViewModel -}}
using System;
{{~ if !Option.SkipLocalization }}
using System.ComponentModel.DataAnnotations;
{{ end ~}}
{{~ if Bag.PagesFolder; pagesNamespace = Bag.PagesFolder + "."; end ~}}

namespace {{ ProjectInfo.FullName }}.Web.Pages.{{ pagesNamespace }}{{ EntityInfo.RelativeNamespace}}.{{ EntityInfo.Name }}.ViewModels
{
    public class Create{{ EntityInfo.Name }}ViewModel
    {
        {{~ for prop in EntityInfo.Properties ~}}
        {{~ if prop | abp.is_ignore_property; continue; end ~}}
        {{~ if !Option.SkipLocalization ~}}
        [Display(Name = "{{ EntityInfo.Name + prop.Name}}")]
        {{~ end ~}}
        {{~ if string.ends_with prop.Type ">" ~}} 
        public {{
        stRgex = prop.Name | regex.replace "s" "." "$"
        stStart = "<" | string.append stRgex
        stFinal = stStart | string.append "ViewModels."
        stFinalEx = stFinal | string.append "Create"
        stRgexTwo = prop.Name | regex.replace "s" "ViewModel" "$"
        stFinalNoEx = stFinalEx | string.append stRgexTwo
        stAppend = stFinalNoEx | string.append ">"
        stRegexFind = prop.Name | regex.replace "s" ">" "$"

        stFind = "<" | string.append stRegexFind


        prop.Type | string.replace stFind stAppend

        }} Create{{ prop.Name }}ViewModel { get; set; }
        {{~ else ~}}
        public {{ prop.Type}} {{ prop.Name }} { get; set; }
        {{~end ~}}
        {{~ if !for.last ~}}

        {{~ end ~}}
        {{~ end ~}}
    }
}