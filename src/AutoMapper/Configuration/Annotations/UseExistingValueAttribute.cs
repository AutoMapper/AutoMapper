using System;

namespace AutoMapper.Configuration.Annotations
{
    public sealed class UseExistingValueAttribute : Attribute, IMemberConfigurationProvider
    {
        public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
        {
            memberConfigurationExpression.UseDestinationValue();
        }
    }
}