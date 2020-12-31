using AutoMapper.Internal;
using System;
using System.Diagnostics;

namespace AutoMapper
{
    [DebuggerDisplay("{RequestedTypes.SourceType.Name}, {RequestedTypes.DestinationType.Name} : {RuntimeTypes.SourceType.Name}, {RuntimeTypes.DestinationType.Name}")]
    public readonly struct MapRequest : IEquatable<MapRequest>
    {
        public readonly TypePair RequestedTypes;
        public readonly TypePair RuntimeTypes;
        public readonly MemberMap MemberMap;
        public MapRequest(in TypePair requestedTypes, in TypePair runtimeTypes, MemberMap memberMap = null) 
        {
            RequestedTypes = requestedTypes;
            RuntimeTypes = runtimeTypes;
            MemberMap = memberMap;
        }
        public bool Equals(MapRequest other) => 
            RequestedTypes.Equals(other.RequestedTypes) && RuntimeTypes.Equals(other.RuntimeTypes) && Equals(MemberMap, other.MemberMap);
        public override bool Equals(object obj) => obj is MapRequest other && Equals(other);
        public override int GetHashCode()
        {
            var hashCode = HashCodeCombiner.Combine(RequestedTypes, RuntimeTypes);
            if(MemberMap != null)
            {
                hashCode = HashCodeCombiner.Combine(hashCode, MemberMap.GetHashCode());
            }
            return hashCode;
        }
        public static bool operator ==(in MapRequest left, in MapRequest right) => left.Equals(right);
        public static bool operator !=(in MapRequest left, in MapRequest right) => !left.Equals(right);
    }
    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public readonly struct TypePair : IEquatable<TypePair>
    {
        public readonly Type SourceType;
        public readonly Type DestinationType;
        public TypePair(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }
        public bool Equals(TypePair other) => SourceType == other.SourceType && DestinationType == other.DestinationType;
        public override bool Equals(object other) => other is TypePair otherPair && Equals(otherPair);
        public override int GetHashCode() => HashCodeCombiner.Combine(SourceType, DestinationType);
        public bool IsConstructedGenericType => SourceType.IsConstructedGenericType || DestinationType.IsConstructedGenericType;
        public bool IsArray => SourceType.IsArray && DestinationType.IsArray;
        public bool IsGenericTypeDefinition => SourceType.IsGenericTypeDefinition || DestinationType.IsGenericTypeDefinition;
        public bool ContainsGenericParameters => SourceType.ContainsGenericParameters || DestinationType.ContainsGenericParameters;
        public TypePair CloseGenericTypes(in TypePair closedTypes)
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
        public TypePair GetTypeDefinitionIfGeneric() => new TypePair(SourceType.GetTypeDefinitionIfGeneric(), DestinationType.GetTypeDefinitionIfGeneric());
        public static bool operator ==(in TypePair left, in TypePair right) => left.Equals(right);
        public static bool operator !=(in TypePair left, in TypePair right) => !left.Equals(right);
    }
    public static class HashCodeCombiner
    {
        public static int Combine<T1, T2>(in T1 obj1, in T2 obj2) => CombineCodes(obj1.GetHashCode(), obj2.GetHashCode());
        public static int CombineCodes(int h1, int h2)
        {
            // RyuJIT optimizes this to use the ROL instruction
            // Related GitHub pull request: https://github.com/dotnet/coreclr/pull/1830
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}