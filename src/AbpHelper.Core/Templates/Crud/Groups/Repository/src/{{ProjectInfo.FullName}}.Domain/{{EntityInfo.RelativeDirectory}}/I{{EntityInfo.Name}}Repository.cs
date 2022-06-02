{{~    
    if EntityInfo.CompositeKeyName
        repository = "IRepository<" + EntityInfo.Name + ">"
    else
        repository = "IRepository<" + EntityInfo.Name + ", " + EntityInfo.PrimaryKey + ">"
    end
~}}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace {{ EntityInfo.Namespace }}
{
    public interface I{{ EntityInfo.Name }}Repository : {{ repository }}
    {
        Task<{{ EntityInfo.Name }}Result> GetListAsync(
                int skipCount,
                int maxResultCount,
                string sorting,
                string filter,
                CancellationToken cancellationToken = default);
}
}