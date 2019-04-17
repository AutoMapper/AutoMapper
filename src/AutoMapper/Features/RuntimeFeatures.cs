using AutoMapper.Internal;

namespace AutoMapper.Features
{
    public class RuntimeFeatures : Features<IFeature>
    {
        internal void Seal(IConfigurationProvider configurationProvider)
        {
            ForAll(feature => feature.Seal(configurationProvider));
            MakeReadOnly();
        }
    }
}