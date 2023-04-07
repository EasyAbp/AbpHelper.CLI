{{~    
    if EntityInfo.CompositeKeyName
    repository = "IRepository<" + EntityInfo.Name + ">"
    else
    repository = "IRepository<" + EntityInfo.Name + ", " + EntityInfo.PrimaryKey + ">"
    end
~}}
using System;
using Volo.Abp.Domain.Repositories;

namespace {{ EntityInfo.Namespace }};

/// <summary>
/// {{ EntityInfo.Document }}
/// </summary>
public interface I{{ EntityInfo.Name }}Repository : {{ repository }}
{
}
