namespace AutoMapper.Features
{
    public class MappingFeatures : Features<IMappingFeature>
    {
        public void ReverseTo(MappingFeatures features) => ForAll(feature =>
        {
            var reverse = feature.Reverse();
            if (reverse != null)
            {
                features.AddOrUpdate(reverse);
            }
        });

        internal void Configure(TypeMap typeMap) => ForAll(feature => feature.Configure(typeMap));
    }
}