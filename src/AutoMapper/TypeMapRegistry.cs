namespace AutoMapper
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Configuration;

    public class TypeMapRegistry
    {
        private readonly IDictionary<TypePair, TypeMap> _typeMaps = new Dictionary<TypePair, TypeMap>();

        public IEnumerable<TypeMap> TypeMaps => _typeMaps.Values;

        public void RegisterTypeMap(TypeMap typeMap) => _typeMaps[typeMap.Types] = typeMap;

        public TypeMap GetTypeMap(TypePair typePair) => _typeMaps.GetOrDefault(typePair);
    }
}