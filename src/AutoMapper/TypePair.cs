using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
    [DebuggerDisplay("{RequestedTypes.SourceType.Name}, {RequestedTypes.DestinationType.Name} : {RuntimeTypes.SourceType.Name}, {RuntimeTypes.DestinationType.Name}")]
    public struct MapRequest : IEquatable<MapRequest>
    {
        public TypePair RequestedTypes { get; }
        public TypePair RuntimeTypes { get; }
        public IMemberMap MemberMap { get; }

        public MapRequest(TypePair requestedTypes, TypePair runtimeTypes, IMemberMap memberMap = null) 
        {
            RequestedTypes = requestedTypes;
            RuntimeTypes = runtimeTypes;
            MemberMap = memberMap;
        }

        public bool Equals(MapRequest other) => 
            RequestedTypes.Equals(other.RequestedTypes) && RuntimeTypes.Equals(other.RuntimeTypes) && Equals(MemberMap, other.MemberMap);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MapRequest && Equals((MapRequest) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = HashCodeCombiner.Combine(RequestedTypes, RuntimeTypes);
            if(MemberMap != null)
            {
                hashCode = HashCodeCombiner.Combine(hashCode, MemberMap.GetHashCode());
            }
            return hashCode;
        }

        public static bool operator ==(MapRequest left, MapRequest right) => left.Equals(right);

        public static bool operator !=(MapRequest left, MapRequest right) => !left.Equals(right);
    }

    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public struct TypePair : IEquatable<TypePair>
    {
        public static bool operator ==(TypePair left, TypePair right) => left.Equals(right);
        public static bool operator !=(TypePair left, TypePair right) => !left.Equals(right);

        public TypePair(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }

        public static TypePair Create<TSource>(TSource source, Type sourceType, Type destinationType)
        {
            if(source != null)
            {
                sourceType = source.GetType();
            }
            return new TypePair(sourceType, destinationType);
        }

        public static TypePair Create<TSource, TDestination>(TSource source, TDestination destination, Type sourceType, Type destinationType)
        {
            if(source != null)
            {
                sourceType = source.GetType();
            }
            if(destination != null)
            {
                destinationType = destination.GetType();
            }
            return new TypePair(sourceType, destinationType);
        }

        public Type SourceType { get; }

        public Type DestinationType { get; }

        public bool Equals(TypePair other) => SourceType == other.SourceType && DestinationType == other.DestinationType;

        public override bool Equals(object other) => other is TypePair && Equals((TypePair)other);

        public override int GetHashCode() => HashCodeCombiner.Combine(SourceType, DestinationType);

        public bool IsGeneric => SourceType.IsGenericType || DestinationType.IsGenericType;

        public bool IsGenericTypeDefinition => SourceType.IsGenericTypeDefinition || DestinationType.IsGenericTypeDefinition;

        public bool ContainsGenericParameters => SourceType.ContainsGenericParameters || DestinationType.ContainsGenericParameters;

        public TypePair? GetOpenGenericTypePair()
        {
            if(!IsGeneric)
            {
                return null;
            }
            var sourceGenericDefinition = SourceType.GetTypeDefinitionIfGeneric();
            var destinationGenericDefinition = DestinationType.GetTypeDefinitionIfGeneric();
            return new TypePair(sourceGenericDefinition, destinationGenericDefinition);
        }

        public TypePair CloseGenericTypes(TypePair closedTypes)
        {
            var sourceArguments = closedTypes.SourceType.GetGenericArguments();
            var destinationArguments = closedTypes.DestinationType.GetGenericArguments();
            if(sourceArguments.Length == 0)
            {
                sourceArguments = destinationArguments;
            }
            else if(destinationArguments.Length == 0)
            {
                destinationArguments = sourceArguments;
            }
            var closedSourceType = SourceType.IsGenericTypeDefinition ? SourceType.MakeGenericType(sourceArguments) : SourceType;
            var closedDestinationType = DestinationType.IsGenericTypeDefinition ? DestinationType.MakeGenericType(destinationArguments) : DestinationType;
            return new TypePair(closedSourceType, closedDestinationType);
        }

        public IEnumerable<TypePair> GetRelatedTypePairs()
        {
            var @this = this;
            var subTypePairs =
                from destinationType in GetAllTypes(DestinationType)
                from sourceType in GetAllTypes(@this.SourceType)
                select new TypePair(sourceType, destinationType);
            return subTypePairs;
        }

        private static IEnumerable<Type> GetAllTypes(Type type)
        {
            var typeInheritance = type.GetTypeInheritance();
            foreach(var item in typeInheritance)
            {
                yield return item;
            }
            var interfaceComparer = new InterfaceComparer(type);
            var allInterfaces = type.GetTypeInfo().ImplementedInterfaces.OrderByDescending(t => t, interfaceComparer);
            foreach(var interfaceType in allInterfaces)
            {
                yield return interfaceType;
            }
        }

        private class InterfaceComparer : IComparer<Type>
        {
            private readonly List<TypeInfo> _typeInheritance;

            public InterfaceComparer(Type target)
            {
                _typeInheritance = target.GetTypeInheritance().Select(type => type.GetTypeInfo()).Reverse().ToList();
            }

            public int Compare(Type x, Type y)
            {
                var xLessOrEqualY = x.IsAssignableFrom(y);
                var yLessOrEqualX = y.IsAssignableFrom(x);

                if (xLessOrEqualY & !yLessOrEqualX)
                {
                    return -1;
                }
                if (!xLessOrEqualY & yLessOrEqualX)
                {
                    return 1;
                }
                if (xLessOrEqualY & yLessOrEqualX)
                {
                    return 0;
                }

                var xFirstIntroduceTypeIndex = _typeInheritance.FindIndex(type => type.ImplementedInterfaces.Contains(x));
                var yFirstIntroduceTypeIndex = _typeInheritance.FindIndex(type => type.ImplementedInterfaces.Contains(y));

                if (xFirstIntroduceTypeIndex < yFirstIntroduceTypeIndex)
                {
                    return -1;
                }
                if (yFirstIntroduceTypeIndex > xFirstIntroduceTypeIndex)
                {
                    return 1;
                }

                return 0;
            }
        }
    }

    public static class HashCodeCombiner
    {
        public static int Combine<T1, T2>(T1 obj1, T2 obj2) =>
            CombineCodes(obj1.GetHashCode(), obj2.GetHashCode());

        public static int CombineCodes(int h1, int h2) => ((h1 << 5) + h1) ^ h2;
    }
}
