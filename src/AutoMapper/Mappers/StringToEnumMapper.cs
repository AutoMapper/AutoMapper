using System;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public class StringToEnumMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);
            return destEnumType != null && context.SourceType == typeof(string);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var destinationType = destExpression.Type;
            var destinationEnumType = TypeHelper.GetEnumerationType(destinationType);
            var enumParse = Expression.Call(typeof(Enum), "Parse", null, Expression.Constant(destinationEnumType), sourceExpression, Expression.Constant(true));
            var isNullOrEmpty = Expression.Call(typeof(string), "IsNullOrEmpty", null, sourceExpression);
            return Expression.Condition(isNullOrEmpty, Expression.Default(destinationType), ExpressionExtensions.ToType(enumParse, destinationType));
        }
    }
}