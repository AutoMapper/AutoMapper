using System;
using System.Reflection;

namespace AutoMapper.Configuration.Annotations
{
    public class ValueResolverAttribute : Attribute, IMemberConfigurationProvider
    {
        /// <summary>
        /// <see cref="IValueResolver{TSource,TDestination,TDestMember}" /> type
        /// </summary>
        public Type Type { get; }

        public ValueResolverAttribute(Type type)
        {
            Type = type;
        }

        public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
        {
            var sourceMemberAttribute = memberConfigurationExpression.DestinationMember.GetCustomAttribute<SourceMemberAttribute>();

            if (sourceMemberAttribute != null)
            {
                memberConfigurationExpression.MapFrom(Type, sourceMemberAttribute.Name);
            }
            else
            {
                memberConfigurationExpression.MapFrom(Type);
            }
        }
    }
}