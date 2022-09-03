using AutoMapper.Internal;
using System;
using System.Reflection;

namespace AutoMapper.Configuration.Annotations;

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
        var destinationMember = memberConfigurationExpression.DestinationMember;
        if (destinationMember.Has<ValueConverterAttribute>() || destinationMember.Has<ValueResolverAttribute>())
        {
            return;
        }
        memberConfigurationExpression.MapFrom(Name);
    }
}