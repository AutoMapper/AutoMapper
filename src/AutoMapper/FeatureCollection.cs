using AutoMapper.Internal;

namespace AutoMapper
{
    public class FeatureCollection : FeatureCollectionBase<IFeature>
    {
        internal void Seal(IConfigurationProvider configurationProvider)
        {
            foreach (var feature in this)
            {
                feature.Value.Seal(configurationProvider);
            }
            MakeReadOnly();
        }
    }
}
