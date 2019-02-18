namespace AutoMapper.Internal
{
    public class FeatureCollection : FeatureCollectionBase<IFeature>, IFeatureCollection
    {
        public void Seal(IConfigurationProvider configurationProvider)
        {
            foreach (var feature in this)
            {
                feature.Value.Seal(configurationProvider);
            }
            MakeReadOnly();
        }
    }
}
