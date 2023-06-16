{{~ if Bag.PagesFolder; pagesNamespace = Bag.PagesFolder + "."; end ~}}
using System;
using System.Threading.Tasks;
{{~ if !Option.SkipGetListInputDto ~}}
using System.ComponentModel.DataAnnotations;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
{{~ end ~}}

namespace {{ ProjectInfo.FullName }}.Web.Pages.{{ pagesNamespace }}{{ EntityInfo.RelativeNamespace }}.{{ EntityInfo.Name }};

public class IndexModel : {{ ProjectInfo.Name }}PageModel
{
    {{~ if !Option.SkipGetListInputDto ~}}
    public {{ EntityInfo.Name }}FilterInput {{ EntityInfo.Name }}Filter { get; set; }
    {{~ end ~}}
    
    public virtual async Task OnGetAsync()
    {
        await Task.CompletedTask;
    }
}

{{~ if !Option.SkipGetListInputDto ~}}
public class {{ EntityInfo.Name }}FilterInput
{
    {{~ for prop in EntityInfo.Properties ~}}
    {{~ if prop | abp.is_ignore_property || string.starts_with prop.Type "List<"; continue; end ~}} 
    [FormControlSize(AbpFormControlSize.Small)]
    {{~ if !Option.SkipLocalization ~}}
    [Display(Name = "{{ EntityInfo.Name + prop.Name}}")]
    {{~ end ~}}
    public {{ prop.Type}}{{~ if !string.ends_with prop.Type "?"; "?"; end}} {{ prop.Name }} { get; set; }
    {{~ if !for.last ~}}

    {{~ end ~}}
    {{~ end ~}}
}
{{~ end ~}}