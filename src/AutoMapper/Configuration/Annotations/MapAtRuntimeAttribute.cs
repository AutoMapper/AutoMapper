using System;

namespace AutoMapper.Configuration.Annotations
{
    public class MapAtRuntimeAttribute : Attribute, IMemberConfigurationProvider
    {
        public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
        {
            memberConfigurationExpression.MapAtRuntime();
        }
    }
}