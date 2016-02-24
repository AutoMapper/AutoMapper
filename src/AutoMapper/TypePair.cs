namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public struct TypePair : IEquatable<TypePair>
    {
        public static bool operator ==(TypePair left, TypePair right) => Equals(left, right);

        public static bool operator !=(TypePair left, TypePair right) => !Equals(left, right);

        public TypePair(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            _hashcode = unchecked (SourceType.GetHashCode() * 397) ^ DestinationType.GetHashCode();
        }

        private readonly int _hashcode;

        public Type SourceType { get; }

        public Type DestinationType { get; }

        public bool Equals(TypePair other) => SourceType == other.SourceType && DestinationType == other.DestinationType;

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) &&
                                                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((TypePair) obj));

        public override int GetHashCode() => _hashcode;

        public TypePair? GetOpenGenericTypePair()
        {
            var isGeneric = SourceType.IsGenericType()
                            && DestinationType.IsGenericType()
                            && (SourceType.GetGenericTypeDefinition() != null)
                            && (DestinationType.GetGenericTypeDefinition() != null);
            if (!isGeneric)
                return null;

            var sourceGenericDefinition = SourceType.GetGenericTypeDefinition();
            var destGenericDefinition = DestinationType.GetGenericTypeDefinition();

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
            foreach (var item in typeInheritance)
                yield return item;

            var interfaceComparer = new InterfaceComparer(type);
            var allInterfaces = type.GetTypeInfo().ImplementedInterfaces.OrderByDescending(t => t, interfaceComparer);

            foreach (var interfaceType in allInterfaces)
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
}
