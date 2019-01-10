using System;

namespace AutoMapper.Configuration.Annotations
{
    public class ValueConverterAttribute : Attribute, IMemberConfigurationProvider
    {
        /// <summary>
        /// <see cref="IValueConverter{TSourceMember,TDestinationMember}" /> type
        /// </summary>
        public Type Type { get; }

        public ValueConverterAttribute(Type type)
        {
            Type = type;
        }

        public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
        {
            memberConfigurationExpression.ConvertUsing(Type);
        }
    }
}