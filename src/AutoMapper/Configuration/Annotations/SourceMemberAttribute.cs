using System;

namespace AutoMapper.Configuration.Annotations
{
    /// <summary>
    /// Specify the source member to map from. Can only reference a member on the <see cref="AutoMapAttribute.SourceType" /> type
    /// </summary>
    /// <remarks>
    /// Must be used in combination with <see cref="AutoMapAttribute" />
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SourceMemberAttribute : Attribute, IMemberConfigurationProvider
    {
        public string Name { get; }

        public SourceMemberAttribute(string name) => Name = name;

        public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
        {
            memberConfigurationExpression.MapFrom(Name);
        }
    }
}