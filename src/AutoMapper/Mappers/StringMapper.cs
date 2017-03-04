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

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var toStringCall = Call(sourceExpression, typeof(object).GetDeclaredMethod("ToString"));
            if(sourceExpression.Type.IsValueType())
            {
                return toStringCall;
            }
            return Condition(Equal(sourceExpression, Constant(null)), Constant(null, typeof(string)), toStringCall);
        }
    }
}