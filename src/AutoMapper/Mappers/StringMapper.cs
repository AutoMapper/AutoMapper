using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using static Expression;

    public class StringMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression)
        {
            var sourceType = sourceExpression.Type;
            var toStringCall = Call(sourceExpression, typeof(object).GetDeclaredMethod("ToString"));
            if(sourceType.IsValueType())
            {
                return toStringCall;
            }
            return Condition(Equal(sourceExpression, Constant(null)), Constant(null, typeof(string)), toStringCall);
        }
    }
}