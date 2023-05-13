using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace {{ EntityInfo.Namespace }};

{{~ if EntityInfo.Document | !string.whitespace ~}}
/// <summary>
/// {{ EntityInfo.Document }}
/// </summary>
{{~ end ~}}
public static class {{ EntityInfo.Name }}EfCoreQueryableExtensions
{
    public static IQueryable<{{ EntityInfo.Name }}> IncludeDetails(this IQueryable<{{ EntityInfo.Name }}> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable
            // .Include(x => x.xxx) // TODO: AbpHelper generated
            ;
    }
}
