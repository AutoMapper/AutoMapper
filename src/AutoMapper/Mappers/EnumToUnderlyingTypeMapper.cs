using System;
using System.Linq.Expressions;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    public class EnumToUnderlyingTypeMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => ElementTypeHelper.IsEnumToUnderlyingType(context);
        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression, 
            Expression destExpression, Expression contextExpression) => sourceExpression;
    }
}