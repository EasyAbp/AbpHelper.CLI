namespace EasyAbp.AbpHelper.Models
{
    public class ParameterInfo
    {
        public string Type { get; }
        
        public string Name { get; }

        public ParameterInfo(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}