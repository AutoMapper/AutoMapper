using AutoMapper.Internal;

namespace AutoMapper.Features
{
    public class FeatureCollection : FeatureCollectionBase<IFeature>
    {
        internal void Seal(IConfigurationProvider configurationProvider)
        {
            ForAll(feature => feature.Seal(configurationProvider));
            MakeReadOnly();
        }
    }
}