using System;
using System.Reflection;

namespace AutoMapper.Configuration.Annotations
{
    /// <summary>
    /// Map destination member using a custom value resolver.
    /// Use with <see cref="SourceMemberAttribute" /> to specify an <see cref="IMemberValueResolver{TSource,TDestination,TSourceMember,TDestMember}" /> type.
    /// </summary>
    /// <remarks>
    /// Must be used in combination with <see cref="AutoMapAttribute" />
    /// </remarks>
    public sealed class ValueResolverAttribute : Attribute, IMemberConfigurationProvider
    {
        /// <summary>
        /// <see cref="IValueResolver{TSource,TDestination,TDestMember}" /> or <see cref="IMemberValueResolver{TSource,TDestination,TSourceMember,TDestMember}" /> type
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