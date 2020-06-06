using System;
using {{ ProjectInfo.FullName }}.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace {{ EntityInfo.Namespace }}
{
{{~
    if EntityInfo.CompositeKeyName
        primaryKeyText = ""
    else
        primaryKeyText = ", " + EntityInfo.PrimaryKey
    end
~}}
    public class {{ EntityInfo.Name }}Repository : EfCoreRepository<{{ ProjectInfo.Name }}DbContext, {{ EntityInfo.Name }}{{ primaryKeyText }}>, I{{ EntityInfo.Name }}Repository
    {
        public {{ EntityInfo.Name }}Repository(IDbContextProvider<{{ ProjectInfo.Name }}DbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}