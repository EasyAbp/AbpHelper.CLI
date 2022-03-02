{{~
if ProjectInfo.TemplateType == 'Module'
    dbContextName = "I" + ProjectInfo.Name + "DbContext"
else
    dbContextName = ProjectInfo.Name + "DbContext"
end
~}}
using System;
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

        public async Task<List<{{ EntityInfo.Name }}>> GetListAsync(
            int skipCount,
            int maxResultCount,
            string sorting,
            string filter,
            CancellationToken cancelationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                //.WhereIf(filter != null, p => p.Name.Contains(filter))
                .PageBy(skipCount, maxResultCount)
                .OrderBy(sorting == null ? nameof({{ EntityInfo.Name }}.Id) : sorting)
                .ToListAsync(cancelationToken);
        }
    }
}