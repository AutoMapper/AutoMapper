namespace AutoMapper
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class TypeMapRegistry
    {
        private readonly ConcurrentDictionary<TypePair, TypeMap> _typeMaps = new ConcurrentDictionary<TypePair, TypeMap>();

        public IEnumerable<TypeMap> TypeMaps => _typeMaps.Values;

        public void RegisterTypeMap(TypeMap typeMap) => _typeMaps.AddOrUpdate(typeMap.Types, typeMap, (tp, tm) => tm);

        public TypeMap GetTypeMap(TypePair typePair)
        {
            TypeMap typeMap;

            return _typeMaps.TryGetValue(typePair, out typeMap) ? typeMap : null;
        }
    }
}