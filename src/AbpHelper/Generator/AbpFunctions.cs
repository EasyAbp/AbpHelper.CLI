using System;
using System.Linq;
using Scriban.Runtime;

namespace EasyAbp.AbpHelper.Generator
{
    public class AbpFunctions : ScriptObject
    {
        public static string CamelCase(string text)
        {
            var parts = text.Split('.')
                .Select(part => part.ToCamelCase());
            return string.Join('.', parts);
        }
    }
}