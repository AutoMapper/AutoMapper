using System;

namespace AutoMapper.Configuration.Annotations;

/// <summary>
/// Supply a custom mapping order instead of what the .NET runtime returns
/// </summary>
/// <remarks>
/// Must be used in combination with <see cref="AutoMapAttribute" />
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class MappingOrderAttribute : Attribute, IMemberConfigurationProvider
{
    public int Value { get; }

    public MappingOrderAttribute(int value)
    {
        Value = value;
    }

    public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
    {
        memberConfigurationExpression.SetMappingOrder(Value);
    }
}