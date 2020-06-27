using System;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class EnumToUnderlyingTypeMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.IsEnumToUnderlyingType();
        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression, 
            Expression destExpression, Expression contextExpression) => sourceExpression;
    }
}