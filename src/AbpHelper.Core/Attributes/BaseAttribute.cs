using System;

namespace EasyAbp.AbpHelper.Core.Attributes
{
    public abstract class BaseAttribute : Attribute
    {
        public string Name { get; }

        public object Default { get; set; } = null!;

        public string Description { get; set; } = null!;

        protected internal BaseAttribute(string name)
        {
            Name = name;
        }
    }
}