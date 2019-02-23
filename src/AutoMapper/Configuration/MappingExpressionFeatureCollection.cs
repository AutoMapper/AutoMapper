using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    public class MappingExpressionFeatureCollection : FeatureCollectionBase<IMappingExpressionFeature>
    {
        internal MappingExpressionFeatureCollection ReverseMap()
        {
            var reverse = new MappingExpressionFeatureCollection();
            foreach (var feature in this)
            {
                reverse[feature.Key] = feature.Value.Reverse();
            }
            return reverse;
        }
    }
}
