namespace AutoMapper.Features
{
    public interface IRuntimeFeature
    {
        void Seal(IConfigurationProvider configurationProvider);
    }

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
    }
}
