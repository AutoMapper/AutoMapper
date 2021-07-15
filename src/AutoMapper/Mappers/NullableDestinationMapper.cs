using System;
using System.Linq.Expressions;
using AutoMapper.Execution;
namespace AutoMapper.Internal.Mappers
{
    public class NullableDestinationMapper : IObjectMapperInfo
    {
        public bool IsMatch(in TypePair context) => context.DestinationType.IsNullableType();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            configurationProvider.MapExpression(profileMap, GetAssociatedTypes(sourceExpression.Type, destExpression.Type), sourceExpression, memberMap);
        public TypePair GetAssociatedTypes(in TypePair initialTypes) => GetAssociatedTypes(initialTypes.SourceType, initialTypes.DestinationType);
        TypePair GetAssociatedTypes(Type sourceType, Type destinationType) => new(sourceType, Nullable.GetUnderlyingType(destinationType));
    }
}