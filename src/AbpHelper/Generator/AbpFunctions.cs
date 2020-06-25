using System;
using System.Linq;
using EasyAbp.AbpHelper.Models;
using Scriban.Runtime;
using Volo.Abp.Http;

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
        
        public static bool IsIgnoreProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo.Type == "Guid?" && propertyInfo.Name == "TenantId") return true;
            return false;
        }

        public static string GetHttpVerb(string methodName)
        {
            string verb = HttpMethodHelper.GetConventionalVerbForMethodName(methodName);
            return $"Http{Char.ToUpper(verb[0])}{verb.Substring(1).ToLower()}";
        }
    }
}