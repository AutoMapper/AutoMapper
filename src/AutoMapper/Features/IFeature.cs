namespace AutoMapper.Features
{
    public interface IFeature
    {
        void Seal(IConfigurationProvider configurationProvider);
    }

    public static class FeatureExtensions
    {
        public static IMapperConfigurationExpression AddOrUpdateFeature(this IMapperConfigurationExpression configuration, IGlobalFeature feature)
        {
            configuration.Features.AddOrUpdate(feature);
            return configuration;
        }

        public static IMappingExpression<TSource, TDestination> AddOrUpdateFeature<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mapping, IMappingFeature feature)
        {
            mapping.Features.AddOrUpdate(feature);
            return mapping;
        }
    }
}
