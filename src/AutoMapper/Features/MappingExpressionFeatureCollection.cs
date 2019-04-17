namespace AutoMapper.Features
{
    public class MappingExpressionFeatureCollection : FeatureCollectionBase<IMappingFeature>
    {
        internal MappingExpressionFeatureCollection CreateReverseCollection()
        {
            var reverseFeatures = new MappingExpressionFeatureCollection();
            ForAll(feature =>
            {
                var reverse = feature.Reverse();
                if (reverse != null)
                {
                    reverseFeatures.AddOrUpdate(reverse);
                }
            });
            return reverseFeatures;
        }

        internal void Configure(TypeMap typeMap) => ForAll(feature => feature.Configure(typeMap));
    }
}