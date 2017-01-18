namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Configuration;

    [DebuggerDisplay("{RequestedTypes.SourceType.Name}, {RequestedTypes.DestinationType.Name} : {RuntimeTypes.SourceType.Name}, {RuntimeTypes.DestinationType.Name}")]
    public struct MapRequest : IEquatable<MapRequest>
    {
        public TypePair RequestedTypes { get; }
        public TypePair RuntimeTypes { get; }

        public MapRequest(TypePair requestedTypes, TypePair runtimeTypes)
        {
            RequestedTypes = requestedTypes;
            RuntimeTypes = runtimeTypes;
        }

        public bool Equals(MapRequest other)
        {
            return RequestedTypes.Equals(other.RequestedTypes) && RuntimeTypes.Equals(other.RuntimeTypes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MapRequest && Equals((MapRequest) obj);
        }

        public override int GetHashCode() => HashCodeCombiner.Combine(RequestedTypes, RuntimeTypes);

        public static bool operator ==(MapRequest left, MapRequest right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapRequest left, MapRequest right)
        {
            return !left.Equals(right);
        }
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

        public TypePair? GetOpenGenericTypePair()
        {
            var isGeneric = SourceType.IsGenericType() || DestinationType.IsGenericType();
            if (!isGeneric)
                return null;

            var sourceGenericDefinition = SourceType.IsGenericType() ? SourceType.GetGenericTypeDefinition() : SourceType;
            var destGenericDefinition = DestinationType.IsGenericType() ? DestinationType.GetGenericTypeDefinition() : DestinationType;

            var genericTypePair = new TypePair(sourceGenericDefinition, destGenericDefinition);

            return genericTypePair;
        }

        public IEnumerable<TypePair> GetRelatedTypePairs()
        {
            var @this = this;
            var subTypePairs =
                from destinationType in GetAllTypes(DestinationType)
                from sourceType in @this.GetAllTypes(@this.SourceType)
                select new TypePair(sourceType, destinationType);
            return subTypePairs;
        }

        private IEnumerable<Type> GetAllTypes(Type type)
        {
            var typeInheritance = GetTypeInheritance(type);
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

        private static IEnumerable<Type> GetTypeInheritance(Type type)
        {
            yield return type;

            Type baseType = type.BaseType();
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType();
            }
        }

        private class InterfaceComparer : IComparer<Type>
        {
            private readonly List<TypeInfo> _typeInheritance;

            public InterfaceComparer(Type target)
            {
                _typeInheritance = GetTypeInheritance(target).Select(type => type.GetTypeInfo()).Reverse().ToList();
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
