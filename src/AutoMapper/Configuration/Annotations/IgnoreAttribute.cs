using System;

namespace AutoMapper.Configuration.Annotations
{
    public class IgnoreAttribute : Attribute, IMemberConfigurationProvider
    {
        public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
        {
            memberConfigurationExpression.Ignore();
        }
    }
}