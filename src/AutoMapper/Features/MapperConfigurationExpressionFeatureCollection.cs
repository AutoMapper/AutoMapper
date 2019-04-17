namespace AutoMapper.Features
{
    public class MapperConfigurationExpressionFeatureCollection : FeatureCollectionBase<IGlobalFeature>
    {
        internal void Configure(MapperConfiguration mapperConfiguration) => ForAll(feature => feature.Configure(mapperConfiguration));
    }
}