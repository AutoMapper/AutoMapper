using System;
using System.Linq.Expressions;
using AutoMapper.Execution;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class NullableDestinationMapper : IObjectMapperInfo
    {
        public bool IsMatch(in TypePair context) => context.DestinationType.IsNullableType();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            ExpressionBuilder.MapExpression(configurationProvider, profileMap,
                new TypePair(sourceExpression.Type, Nullable.GetUnderlyingType(destExpression.Type)),
                sourceExpression,
                memberMap
            );
        public TypePair GetAssociatedTypes(in TypePair initialTypes) => new TypePair(initialTypes.SourceType, Nullable.GetUnderlyingType(initialTypes.DestinationType));
    }
}