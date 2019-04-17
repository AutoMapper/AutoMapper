using AutoMapper.Internal;

namespace AutoMapper.Features
{
    public class RuntimeFeatureCollection : FeatureCollectionBase<IFeature>
    {
        internal void Seal(IConfigurationProvider configurationProvider)
        {
            ForAll(feature => feature.Seal(configurationProvider));
            MakeReadOnly();
        }
    }
}