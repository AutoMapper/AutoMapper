using AutoMapper.Internal;
using System.Linq;

namespace AutoMapper.Configuration
{
    public class MappingExpressionFeatureCollection : FeatureCollectionBase<IMappingExpressionFeature>
    {
        internal MappingExpressionFeatureCollection CreateReverseCollection()
        {
            var reverseFeatures = new MappingExpressionFeatureCollection();
            foreach (var reverse in this.Select(f=>f.Reverse()).Where(f=>f!=null))
            {
                reverseFeatures.AddOrUpdate(reverse);
            }
            return reverseFeatures;
        }

        internal void Configure(TypeMap typeMap) => ForAll(feature => feature.Configure(typeMap));
    }
}