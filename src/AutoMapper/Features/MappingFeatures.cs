using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Features
{
    public class MappingFeatures : Features<IMappingFeature>
    {
        public void ReverseTo(MappingFeatures features) => features.AddOrUpdateRange(CreateReverseCollection());

        private IEnumerable<IMappingFeature> CreateReverseCollection() => this.Select(f => f.Reverse()).Where(f => f != null);

        internal void Configure(TypeMap typeMap) => ForAll(feature => feature.Configure(typeMap));
    }
}