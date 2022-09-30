namespace AutoMapper.Configuration.Annotations;

/// <summary>
/// Use the destination value instead of mapping from the source value or creating a new instance
/// </summary>
/// <remarks>
/// Must be used in combination with <see cref="AutoMapAttribute" />
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class UseExistingValueAttribute : Attribute, IMemberConfigurationProvider
{
    public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
    {
        memberConfigurationExpression.UseDestinationValue();
    }
}