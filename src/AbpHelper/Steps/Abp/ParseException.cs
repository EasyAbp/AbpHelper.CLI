using System;
using System.Collections.Generic;

namespace DosSEdo.AbpHelper.Steps.Abp
{
    public class ParseException : Exception
    {
        public ParseException(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }

        public List<string> Errors { get; } = new List<string>();
    }
}