using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public class StringMapper : IObjectMapper, IObjectMapExpression
    {
        public object Map(ResolutionContext context)
        {
            return context.SourceValue?.ToString();
        }

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Condition(Expression.Equal(sourceExpression, Expression.Default(sourceExpression.Type)),
                Expression.Constant(null, typeof (string)),
                Expression.Call(sourceExpression, typeof (object).GetMethod("ToString")));
        }
    }
}