{{-
    dtoType = DtoInfo.CreateTypeName
    if Option.SkipViewModel
        viewModelType = dtoType
    else
        if Option.SeparateDto
            viewModelType = "Create" + EntityInfo.Name + "ViewModel"
        else
            viewModelType = "CreateEdit" + EntityInfo.Name + "ViewModel"
        end
    end
-}}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using {{ EntityInfo.Namespace }};
using {{ EntityInfo.Namespace }}.Dtos;
{{~ if Bag.PagesFolder; pagesNamespace = Bag.PagesFolder + "."; end ~}}
{{~ if !Option.SkipViewModel ~}}
using {{ ProjectInfo.FullName }}.Web.Pages.{{ pagesNamespace }}{{ EntityInfo.RelativeNamespace}}.{{ EntityInfo.Name }}.ViewModels;
{{~ end ~}}

namespace {{ ProjectInfo.FullName }}.Web.Pages.{{ pagesNamespace }}{{ EntityInfo.RelativeNamespace}}.{{ EntityInfo.Name }}
{
    public class CreateModalModel : {{ ProjectInfo.Name }}PageModel
    {
        [BindProperty]
        public {{ viewModelType }} ViewModel { get; set; }

        private readonly I{{ EntityInfo.Name }}AppService _service;

        public CreateModalModel(I{{ EntityInfo.Name }}AppService service)
        {
            _service = service;
        }

        public virtual async Task<IActionResult> OnPostAsync()
        {
{{~ if Option.SkipViewModel ~}}    
            await _service.CreateAsync(ViewModel);
{{~ else ~}}
            var dto = ObjectMapper.Map<{{ viewModelType }}, {{ dtoType }}>(ViewModel);
            await _service.CreateAsync(dto);
{{~ end ~}}
            return NoContent();
        }
    }
}