using System;
using System.Linq;
using DosSEdo.AbpHelper.Models;
using Scriban.Runtime;

namespace DosSEdo.AbpHelper.Generator
{
    public class AbpFunctions : ScriptObject
    {
        public static string CamelCase(string text)
        {
            System.Collections.Generic.IEnumerable<string> parts = text.Split('.')
                .Select(part => part.ToCamelCase());
            return string.Join('.', parts);
        }
        
        public static bool IsIgnoreProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo.Type == "Guid?" && propertyInfo.Name == "TenantId") return true;
            return false;
        }
    }
}