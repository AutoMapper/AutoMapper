using System;

namespace AutoMapper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class AutoMapAttribute : Attribute
    {
        public AutoMapAttribute(Type sourceType) 
            => SourceType = sourceType;

        public Type SourceType { get; set; }
    }
}