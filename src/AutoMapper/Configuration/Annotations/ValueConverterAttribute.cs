using System;
using System.Reflection;

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
            var sourceMemberAttribute = memberConfigurationExpression.DestinationMember.GetCustomAttribute<SourceMemberAttribute>();

            if (sourceMemberAttribute != null)
            {
                memberConfigurationExpression.ConvertUsing(Type, sourceMemberAttribute.Name);
            }
            else
            {
                memberConfigurationExpression.ConvertUsing(Type);
            }
        }
    }
}