using System;

namespace AutoMapper.Configuration.Annotations
{
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