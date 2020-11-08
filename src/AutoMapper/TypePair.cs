using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
    [DebuggerDisplay("{RequestedTypes.SourceType.Name}, {RequestedTypes.DestinationType.Name} : {RuntimeTypes.SourceType.Name}, {RuntimeTypes.DestinationType.Name}")]
    public readonly struct MapRequest : IEquatable<MapRequest>
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

        public static bool operator ==(MapRequest left, MapRequest right) => left.Equals(right);

        public static bool operator !=(MapRequest left, MapRequest right) => !left.Equals(right);
    }

    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public readonly struct TypePair : IEquatable<TypePair>
    {
        public static bool operator ==(TypePair left, TypePair right) => left.Equals(right);
        public static bool operator !=(TypePair left, TypePair right) => !left.Equals(right);

        public TypePair(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }

        public Type SourceType { get; }

        public Type DestinationType { get; }

        public bool Equals(TypePair other) => SourceType == other.SourceType && DestinationType == other.DestinationType;

        public override bool Equals(object other) => other is TypePair otherPair && Equals(otherPair);

        public override int GetHashCode() => HashCodeCombiner.Combine(SourceType, DestinationType);

        public bool IsGeneric => SourceType.IsGenericType || DestinationType.IsGenericType;

        public bool IsArray => SourceType.IsArray && DestinationType.IsArray;

        public bool IsGenericTypeDefinition => SourceType.IsGenericTypeDefinition || DestinationType.IsGenericTypeDefinition;

        public bool ContainsGenericParameters => SourceType.ContainsGenericParameters || DestinationType.ContainsGenericParameters;

        public TypePair GetOpenGenericTypePair()
        {
            if(!IsGeneric)
            {
                return default;
            }
            var sourceGenericDefinition = SourceType.GetTypeDefinitionIfGeneric();
            var destinationGenericDefinition = DestinationType.GetTypeDefinitionIfGeneric();
            return new TypePair(sourceGenericDefinition, destinationGenericDefinition);
        }

        public bool IsEmpty => SourceType == null;

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

        public void CheckIsDerivedFrom(in TypePair baseTypes)
        {
            SourceType.CheckIsDerivedFrom(baseTypes.SourceType);
            DestinationType.CheckIsDerivedFrom(baseTypes.DestinationType);
        }
    }
    public static class HashCodeCombiner
    {
        public static int Combine<T1, T2>(T1 obj1, T2 obj2) => CombineCodes(obj1.GetHashCode(), obj2.GetHashCode());
        public static int CombineCodes(int h1, int h2)
        {
            // RyuJIT optimizes this to use the ROL instruction
            // Related GitHub pull request: https://github.com/dotnet/coreclr/pull/1830
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}