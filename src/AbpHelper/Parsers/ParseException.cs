using System;
using System.Collections.Generic;

namespace AbpHelper.Parsers
{
    public class ParseException : Exception
    {
        public List<string> Errors { get; } = new List<string>();

        public ParseException(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }
    }
}