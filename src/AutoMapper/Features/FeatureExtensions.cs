using AutoMapper.Internal;

namespace AutoMapper.Features
{
    public static class FeatureExtensions
    {
        public static IMapperConfigurationExpression SetFeature(this IMapperConfigurationExpression configuration, IGlobalFeature feature)
        {
            configuration.Features.Set(feature);
            return configuration;
        }

        public static IMappingExpression<TSource, TDestination> SetFeature<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mapping, IMappingFeature feature)
        {
            mapping.Features.Set(feature);
            return mapping;
        }

        internal static void Configure(this Features<IGlobalFeature> features, MapperConfiguration mapperConfiguration) => features.ForAll(feature => feature.Configure(mapperConfiguration));

        public static void ReverseTo(this Features<IMappingFeature> features, Features<IMappingFeature> reversedFeatures) => features.ForAll(feature =>
        {
            var reverse = feature.Reverse();
            if (reverse != null)
            {
                reversedFeatures.Set(reverse);
            }
        });

        internal static void Configure(this Features<IMappingFeature> features, TypeMap typeMap) => features.ForAll(feature => feature.Configure(typeMap));

        internal static void Seal(this Features<IRuntimeFeature> features, IConfigurationProvider configurationProvider)
        {
            features.ForAll(feature => feature.Seal(configurationProvider));
            features.MakeReadOnly();
        }
    }
}