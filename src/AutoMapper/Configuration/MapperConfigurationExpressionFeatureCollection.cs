using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    public class MapperConfigurationExpressionFeatureCollection : FeatureCollectionBase<IMapperConfigurationExpressionFeature>
    {
        internal void Configure(MapperConfiguration mapperConfiguration) => ForAll(feature => feature.Configure(mapperConfiguration));
    }
}