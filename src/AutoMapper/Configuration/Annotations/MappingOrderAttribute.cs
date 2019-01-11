using System;

namespace AutoMapper.Configuration.Annotations
{
    public sealed class MappingOrderAttribute : Attribute, IMemberConfigurationProvider
    {
        public int Value { get; }

        public MappingOrderAttribute(int value)
        {
            Value = value;
        }

        public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
        {
            memberConfigurationExpression.SetMappingOrder(Value);
        }
    }
}