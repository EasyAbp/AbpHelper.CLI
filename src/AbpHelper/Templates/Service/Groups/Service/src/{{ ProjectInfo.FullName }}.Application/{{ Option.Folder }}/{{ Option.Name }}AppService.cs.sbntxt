{{~ if Option.Folder ~}}
namespace {{ ProjectInfo.FullName }}.{{ Option.Folder | string.replace "/" "." }}
{{~ else ~}}
namespace {{ ProjectInfo.FullName }}
{{~ end ~}}
{
    public class {{ Option.Name }}AppService : {{ ProjectInfo.Name }}AppService, I{{ Option.Name }}AppService
    {
    }
}