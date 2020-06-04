using System;
using System.Linq.Expressions;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    public class UnderlyingTypeToEnumMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            var destEnumType = ElementTypeHelper.GetEnumerationType(context.DestinationType);
            return destEnumType != null && context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(destEnumType));
        }
        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression,
            Expression destExpression, Expression contextExpression) => sourceExpression;
    }
}