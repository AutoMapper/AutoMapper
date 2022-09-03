using System.Collections;
namespace AutoMapper.Features;
public interface IGlobalFeature
{
    void Configure(IGlobalConfiguration configuration);
}
public interface IMappingFeature
{
    void Configure(TypeMap typeMap);
    IMappingFeature Reverse();
}
public interface IRuntimeFeature
{
    void Seal(IGlobalConfiguration configuration);
}
public class Features<TFeature> : IReadOnlyCollection<TFeature>
{
    private Dictionary<Type, TFeature> _features;
    public int Count => _features?.Count ?? 0;
    /// <summary>
    /// Gets the feature of type <typeparamref name="TFeatureToFind"/>.
    /// </summary>
    /// <typeparam name="TFeatureToFind">The type of the feature.</typeparam>
    /// <returns>The feature or null if feature not exists.</returns>
    public TFeatureToFind Get<TFeatureToFind>() where TFeatureToFind : TFeature =>
        _features == null ? default : (TFeatureToFind)_features.GetValueOrDefault(typeof(TFeatureToFind));
    /// <summary>
    /// Add or update the feature. Existing feature of the same type will be replaced.
    /// </summary>
    /// <param name="feature">The feature.</param>
    public void Set(TFeature feature)
    {
        _features ??= new Dictionary<Type, TFeature>();
        _features[feature.GetType()] = feature;
    }
    public IEnumerator<TFeature> GetEnumerator() =>
        _features == null ? Enumerable.Empty<TFeature>().GetEnumerator() : _features.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
public static class FeatureExtensions
{
    public static IMapperConfigurationExpression SetFeature(this IMapperConfigurationExpression configuration, IGlobalFeature feature)
    {
        configuration.Internal().Features.Set(feature);
        return configuration;
    }
    public static IMappingExpression<TSource, TDestination> SetFeature<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mapping, IMappingFeature feature)
    {
        mapping.Features.Set(feature);
        return mapping;
    }
    internal static void Configure(this Features<IGlobalFeature> features, MapperConfiguration mapperConfiguration)
    {
        if (features.Count == 0)
        {
            return;
        }
        foreach (var feature in features)
        {
            feature.Configure(mapperConfiguration);
        }
    }
    public static void ReverseTo(this Features<IMappingFeature> features, Features<IMappingFeature> reversedFeatures)
    {
        if (features.Count == 0)
        {
            return;
        }
        foreach (var feature in features)
        {
            var reverse = feature.Reverse();
            if (reverse != null)
            {
                reversedFeatures.Set(reverse);
            }
        }
    }
    internal static void Configure(this Features<IMappingFeature> features, TypeMap typeMap)
    {
        if (features.Count == 0)
        {
            return;
        }
        foreach (var feature in features)
        {
            feature.Configure(typeMap);
        }
    }
    internal static void Seal(this Features<IRuntimeFeature> features, IGlobalConfiguration configuration)
    {
        if (features.Count == 0)
        {
            return;
        }
        foreach (var feature in features)
        {
            feature.Seal(configuration);
        }
    }
}