using System;
using System.Linq.Expressions;
using AutoMapper.Internal;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static ExpressionFactory;

    public class StringToEnumMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            var destEnumType = ElementTypeHelper.GetEnumerationType(context.DestinationType);
            return destEnumType != null && context.SourceType == typeof(string);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var destinationType = destExpression.Type;
            var destinationEnumType = ElementTypeHelper.GetEnumerationType(destinationType);
            var enumParse = Expression.Call(typeof(Enum), "Parse", null, Expression.Constant(destinationEnumType), sourceExpression, Expression.Constant(true));
            var isNullOrEmpty = Expression.Call(typeof(string), "IsNullOrEmpty", null, sourceExpression);
            return Expression.Condition(isNullOrEmpty, Expression.Default(destinationType), ToType(enumParse, destinationType));
        }
    }
}