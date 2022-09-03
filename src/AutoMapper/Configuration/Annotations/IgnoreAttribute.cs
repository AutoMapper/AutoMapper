namespace AutoMapper.Configuration.Annotations;

/// <summary>
/// Ignore this member for configuration validation and skip during mapping.
/// </summary>
/// <remarks>
/// Must be used in combination with <see cref="AutoMapAttribute" />
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class IgnoreAttribute : Attribute, IMemberConfigurationProvider
{
    public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
    {
        memberConfigurationExpression.Ignore();
    }
}