using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    public class MappingExpressionFeatureCollection : FeatureCollectionBase<IMappingExpressionFeature>
    {
        internal MappingExpressionFeatureCollection CreateReverseCollection()
        {
            var reverse = new MappingExpressionFeatureCollection();
            foreach (var feature in this)
            {
                reverse[feature.Key] = feature.Value.Reverse();
            }
            return reverse;
        }

        internal void Configure(TypeMap typeMap)
        {
            foreach (var item in this)
            {
                item.Value.Configure(typeMap);
            }
        }
    }
}
