using Newtonsoft.Json;

namespace EasyAbp.AbpHelper.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToJson(this object obj, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(obj, formatting);
        }
    }
}