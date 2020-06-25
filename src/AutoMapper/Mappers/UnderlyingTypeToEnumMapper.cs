using System;
using System.Linq.Expressions;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    public class UnderlyingTypeToEnumMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => ElementTypeHelper.IsUnderlyingTypeToEnum(context);
        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression,
            Expression destExpression, Expression contextExpression) => sourceExpression;
    }
}