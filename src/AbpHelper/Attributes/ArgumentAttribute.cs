using System;

namespace EasyAbp.AbpHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentAttribute : BaseAttribute
    {
        public ArgumentAttribute(string name) : base(name)
        {
        }
    }
}
