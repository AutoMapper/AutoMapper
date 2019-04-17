namespace AutoMapper.Features
{
    public class GlobalFeatureCollection : FeatureCollectionBase<IGlobalFeature>
    {
        internal void Configure(MapperConfiguration mapperConfiguration) => ForAll(feature => feature.Configure(mapperConfiguration));
    }
}