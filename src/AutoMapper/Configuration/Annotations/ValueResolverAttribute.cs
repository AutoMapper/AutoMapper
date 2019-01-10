using System;

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
            memberConfigurationExpression.MapFrom(Type);
        }
    }
}