using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    public class MapperConfigurationExpressionFeatureCollection : FeatureCollectionBase<IMapperConfigurationExpressionFeature>
    {
        internal void Configure(MapperConfiguration mapperConfiguration)
        {
            foreach(var item in this)
            {
                item.Value.Configure(mapperConfiguration);
            }
        }
    }
}
