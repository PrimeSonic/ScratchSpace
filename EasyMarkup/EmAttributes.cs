namespace EasyMarkup
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EasyMarkupProperty : Attribute
    {
        public object DefaultValue { get; set; }

        public string Key { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EasyMarkupClass : Attribute
    {
    }
}