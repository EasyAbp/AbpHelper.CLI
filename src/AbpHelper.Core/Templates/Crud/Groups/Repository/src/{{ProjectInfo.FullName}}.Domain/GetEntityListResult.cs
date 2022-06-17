﻿using System.Collections.Generic;

namespace {{ EntityInfo.Namespace }}
{
    public class GetEntityListResult<T>
    {
        public List<T> Items { get; set; }
        public long TotalCount { get; set; }
    }
}