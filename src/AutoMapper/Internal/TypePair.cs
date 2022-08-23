using System;
using System.Diagnostics;
namespace AutoMapper.Internal
{
    [DebuggerDisplay("{RequestedTypes.SourceType.Name}, {RequestedTypes.DestinationType.Name} : {RuntimeTypes.SourceType.Name}, {RuntimeTypes.DestinationType.Name}")]
    public readonly record struct MapRequest(TypePair RequestedTypes, TypePair RuntimeTypes, MemberMap MemberMap)
    {
        public bool Equals(MapRequest other) => RequestedTypes.Equals(other.RequestedTypes) && RuntimeTypes.Equals(other.RuntimeTypes);
        public override int GetHashCode() => HashCode.Combine(RequestedTypes, RuntimeTypes);
    }
    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public readonly record struct TypePair(Type SourceType, Type DestinationType)
    {
        public bool IsConstructedGenericType => SourceType.IsConstructedGenericType || DestinationType.IsConstructedGenericType;
        public bool ContainsGenericParameters => SourceType.ContainsGenericParameters || DestinationType.ContainsGenericParameters;
        public TypePair CloseGenericTypes(TypePair closedTypes)
        {
            var sourceArguments = closedTypes.SourceType.GenericTypeArguments;
            var destinationArguments = closedTypes.DestinationType.GenericTypeArguments;
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
        public Type ITypeConverter() => ContainsGenericParameters ? null : typeof(ITypeConverter<,>).MakeGenericType(SourceType, DestinationType);
        public TypePair GetTypeDefinitionIfGeneric() => new(GetTypeDefinitionIfGeneric(SourceType), GetTypeDefinitionIfGeneric(DestinationType));
        private static Type GetTypeDefinitionIfGeneric(Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() : type;
    }
}