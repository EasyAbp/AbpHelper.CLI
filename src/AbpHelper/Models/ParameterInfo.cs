namespace EasyAbp.AbpHelper.Models
{
    public class ParameterInfo
    {
        public string Type { get; }
        
        public string FullType { get; }
        
        public string Name { get; }

        public ParameterInfo(string type, string fullType, string name)
        {
            Type = type;
            FullType = fullType;
            Name = name;
        }
    }
}