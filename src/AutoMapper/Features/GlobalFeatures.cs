namespace AutoMapper.Features
{
    public class GlobalFeatures : Features<IGlobalFeature>
    {
        internal void Configure(MapperConfiguration mapperConfiguration) => ForAll(feature => feature.Configure(mapperConfiguration));
    }
}