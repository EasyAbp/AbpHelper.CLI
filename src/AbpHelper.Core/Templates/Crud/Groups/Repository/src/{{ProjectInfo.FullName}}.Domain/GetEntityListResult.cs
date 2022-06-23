using System.Collections.Generic;

namespace {{ ProjectInfo.FullName }}
{
    public class GetEntityListResult<T>
    {
        public List<T> Items { get; set; }
        public long TotalCount { get; set; }
    }
}