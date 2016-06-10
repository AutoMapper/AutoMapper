using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public class StringMapper : IObjectMapExpression
    {
        public object Map(ResolutionContext context)
        {
            return context.SourceValue?.ToString();
        }

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Condition(Expression.Equal(sourceExpression, Expression.Default(sourceExpression.Type)),
                Expression.Constant(null, typeof (string)),
                Expression.Call(sourceExpression, typeof (object).GetMethod("ToString")));
        }
    }
}