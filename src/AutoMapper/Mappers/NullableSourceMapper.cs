using System;
using System.Linq.Expressions;
using AutoMapper.Execution;
namespace AutoMapper.Internal.Mappers
{
    public class NullableSourceMapper : IObjectMapperInfo
    {
        public bool IsMatch(in TypePair context) => context.SourceType.IsNullableType();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            configurationProvider.MapExpression(profileMap, GetAssociatedTypes(sourceExpression.Type, destExpression.Type),
                    ExpressionBuilder.Property(sourceExpression, "Value"), memberMap, destExpression);
        public TypePair GetAssociatedTypes(in TypePair initialTypes) => GetAssociatedTypes(initialTypes.SourceType, initialTypes.DestinationType);
        TypePair GetAssociatedTypes(Type sourceType, Type destinationType) => new(Nullable.GetUnderlyingType(sourceType), destinationType);
    }
}