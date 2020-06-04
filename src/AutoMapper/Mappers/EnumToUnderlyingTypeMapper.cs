using System;
using System.Linq.Expressions;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    public class EnumToUnderlyingTypeMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = ElementTypeHelper.GetEnumerationType(context.SourceType);
            return sourceEnumType != null && context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(sourceEnumType));
        }
        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression, 
            Expression destExpression, Expression contextExpression) => sourceExpression;
    }
}