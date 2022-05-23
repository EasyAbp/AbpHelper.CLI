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

namespace {{ EntityInfo.Namespace }};

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
}