using System;

namespace EasyAbp.AbpHelper.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionAttribute : BaseAttribute
    {
        public string ShortName { get; } = null!;

        public bool Required { get; set; }

        public OptionAttribute(char shortName, string name) : base(name)
        {
            ShortName = shortName.ToString();
        }

        public OptionAttribute(string name) : base(name)
        {
        }
    }
}
