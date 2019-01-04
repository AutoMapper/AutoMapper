using System;

namespace AutoMapper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class AutoMapAttribute : Attribute
    {
        public AutoMapAttribute(Type sourceType) 
            => SourceType = sourceType;

        public Type SourceType { get; }
        public bool ReverseMap { get; set; }

        public void ApplyConfiguration(IMappingExpression mappingExpression)
        {
            if (ReverseMap)
            {
                mappingExpression.ReverseMap();
            }
        }
    }
}