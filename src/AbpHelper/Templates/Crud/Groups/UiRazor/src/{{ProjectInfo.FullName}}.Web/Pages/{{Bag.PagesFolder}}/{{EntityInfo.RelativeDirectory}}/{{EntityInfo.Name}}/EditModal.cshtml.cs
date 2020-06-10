{{-
    if Option.SeparateDto
        dtoType = "Update" + EntityInfo.Name + "Dto"
        if Option.SkipViewModel
            viewModelType = dtoType
        else
            viewModelType = "Edit" + EntityInfo.Name + "ViewModel"
        end
    else
        dtoType = "CreateUpdate" + EntityInfo.Name + "Dto"
        if Option.SkipViewModel
            viewModelType = dtoType
        else
            viewModelType = "CreateEdit" + EntityInfo.Name + "ViewModel"
        end
    end
-}}
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using {{ EntityInfo.Namespace }};
using {{ EntityInfo.Namespace }}.Dtos;
{{~ if Bag.PagesFolder; pagesNamespace = Bag.PagesFolder + "."; end ~}}
{{~ if !Option.SkipViewModel ~}}
using {{ ProjectInfo.FullName }}.Web.Pages.{{ pagesNamespace }}{{ EntityInfo.RelativeNamespace}}.{{ EntityInfo.Name }}.ViewModels;
{{~ end ~}}

namespace {{ ProjectInfo.FullName }}.Web.Pages.{{ pagesNamespace }}{{ EntityInfo.RelativeNamespace }}.{{ EntityInfo.Name }}
{
    public class EditModalModel : {{ ProjectInfo.Name }}PageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public {{ EntityInfo.PrimaryKey ?? EntityInfo.CompositeKeyName }} Id { get; set; }

        [BindProperty]
        public {{ viewModelType }} ViewModel { get; set; }

        private readonly I{{ EntityInfo.Name }}AppService _service;

        public EditModalModel(I{{ EntityInfo.Name }}AppService service)
        {
            _service = service;
        }

        public virtual async Task OnGetAsync()
        {
            var dto = await _service.GetAsync(Id);
            ViewModel = ObjectMapper.Map<{{ EntityInfo.Name }}Dto, {{ viewModelType }}>(dto);
        }

        public virtual async Task<IActionResult> OnPostAsync()
        {
{{~ if Option.SkipViewModel ~}}    
            await _service.UpdateAsync(Id, ViewModel);
{{~ else ~}}
            var dto = ObjectMapper.Map<{{ viewModelType }}, {{ dtoType }}>(ViewModel);
            await _service.UpdateAsync(Id, dto);
{{~ end ~}}
            return NoContent();
        }
    }
}