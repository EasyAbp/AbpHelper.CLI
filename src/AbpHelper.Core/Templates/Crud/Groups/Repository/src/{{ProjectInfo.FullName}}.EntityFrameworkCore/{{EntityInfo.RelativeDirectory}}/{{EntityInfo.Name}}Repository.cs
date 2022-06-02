{{~
if ProjectInfo.TemplateType == 'Module'
    dbContextName = "I" + ProjectInfo.Name + "DbContext"
else
    dbContextName = ProjectInfo.Name + "DbContext"
end
~}}
using System;
using System.Linq;
using System.Threading.Tasks;
using {{ ProjectInfo.FullName }}.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace {{ EntityInfo.Namespace }}
{
{{~
    if EntityInfo.CompositeKeyName
        primaryKeyText = ""
    else
        primaryKeyText = ", " + EntityInfo.PrimaryKey
    end
~}}
    public class {{ EntityInfo.Name }}Repository : EfCoreRepository<{{ dbContextName }}, {{ EntityInfo.Name }}{{ primaryKeyText }}>, I{{ EntityInfo.Name }}Repository
    {
        public {{ EntityInfo.Name }}Repository(IDbContextProvider<{{ dbContextName }}> dbContextProvider) : base(dbContextProvider)
        {
        }

        public override async Task<IQueryable<{{ EntityInfo.Name }}>> WithDetailsAsync()
        {
            return (await GetQueryableAsync()).IncludeDetails();
        }


        public async Task<{{ EntityInfo.Name }}Result> GetListAsync(
            int skipCount,
            int maxResultCount,
            string sorting,
            string filter,
            CancellationToken cancellationToken = default)
        {
            var query = await GetDbSetAsync()
                //.WhereIf(filter != null, p => p.Name.Contains(filter))
                ;
            
            return new {{ EntityInfo.Name }}Result()
            {
                Items = await query
                    .PageBy(skipCount, maxResultCount)
                    .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof({{ EntityInfo.Name }}.Id) : sorting)
                    .ToListAsync(cancellationToken),
                TotalCount = await query.CountAsync(cancellationToken)
            };
            
        }
    }
}
