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
    private List<TFeature> _features;
    public int Count => _features?.Count ?? 0;
    /// <summary>
    /// Gets the feature of type <typeparamref name="TFeatureToFind"/>.
    /// </summary>
    /// <typeparam name="TFeatureToFind">The type of the feature.</typeparam>
    /// <returns>The feature or null if feature not exists.</returns>
    public TFeatureToFind Get<TFeatureToFind>() where TFeatureToFind : TFeature =>
        _features == null ? default : (TFeatureToFind)GetFeature(typeof(TFeatureToFind));
    /// <summary>
    /// Add or update the feature. Existing feature of the same type will be replaced.
    /// </summary>
    /// <param name="feature">The feature.</param>
    public void Set(TFeature feature)
    {
        _features ??= new();
        int index = 0;
        var featureType = feature.GetType();
        for (; index < _features.Count && _features[index].GetType() != featureType; index++);
        if (index < _features.Count)
        {
            _features[index] = feature;
        }
        else
        {
            _features.Add(feature);
        }
    }
    object GetFeature(Type featureType)
    {
        foreach (var feature in _features)
        {
            if (feature.GetType() == featureType)
            {
                return feature;
            }
        }
        return null;
    }
    public IEnumerator<TFeature> GetEnumerator() =>
        _features == null ? Enumerable.Empty<TFeature>().GetEnumerator() : _features.GetEnumerator();
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