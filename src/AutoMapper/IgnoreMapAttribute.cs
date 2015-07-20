namespace AutoMapper
{
    using System;

    /// <summary>
    /// Ignore this member for validation and skip during mapping
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreMapAttribute : Attribute
    {
    }
}