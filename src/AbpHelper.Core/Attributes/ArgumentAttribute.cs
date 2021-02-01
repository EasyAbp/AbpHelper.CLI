using System;

namespace EasyAbp.AbpHelper.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentAttribute : BaseAttribute
    {
        public ArgumentAttribute(string name) : base(name)
        {
        }
    }
}
