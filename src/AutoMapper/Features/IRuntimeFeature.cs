namespace AutoMapper.Features
{
    public interface IRuntimeFeature
    {
        void Seal(IConfigurationProvider configurationProvider);
    }

    public static class FeatureExtensions
    {
        public static IMapperConfigurationExpression AddOrUpdateFeature(this IMapperConfigurationExpression configuration, IGlobalFeature feature)
        {
            configuration.Features.Set(feature);
            return configuration;
        }

        public static IMappingExpression<TSource, TDestination> AddOrUpdateFeature<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mapping, IMappingFeature feature)
        {
            mapping.Features.Set(feature);
            return mapping;
        }
    }
}
