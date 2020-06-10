using Volo.Abp.Application.Services;

{{~ if Option.Folder ~}}
namespace {{ ProjectInfo.FullName }}.{{ Option.Folder | string.replace "/" "." }}
{{~ else ~}}
namespace {{ ProjectInfo.FullName }}
{{~ end ~}}
{
    public interface I{{ Option.Name }}AppService : IApplicationService
    {
    }
}