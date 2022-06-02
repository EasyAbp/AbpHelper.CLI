{{~    
    if EntityInfo.CompositeKeyName
        repository = "IRepository<" + EntityInfo.Name + ">"
    else
        repository = "IRepository<" + EntityInfo.Name + ", " + EntityInfo.PrimaryKey + ">"
    end
~}}
using System.Collections.Generic;

namespace {{ EntityInfo.Namespace }}
{
    public class {{ EntityInfo.Name }}Result
    {
        public List<{{ EntityInfo.Name }}> Items { get; set; }
        public long TotalCount { get; set; }
    }
}