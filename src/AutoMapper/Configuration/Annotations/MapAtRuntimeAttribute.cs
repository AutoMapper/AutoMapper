namespace AutoMapper.Configuration.Annotations;

/// <summary>
/// Do not precompute the execution plan for this member, just map it at runtime.
/// Simplifies the execution plan by not inlining.
/// </summary>
/// <remarks>
/// Must be used in combination with <see cref="AutoMapAttribute" />
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class MapAtRuntimeAttribute : Attribute, IMemberConfigurationProvider
{
    public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
    {
        memberConfigurationExpression.MapAtRuntime();
    }
}