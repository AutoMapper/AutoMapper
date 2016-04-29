using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using Configuration;

    public class NullableMapper : IObjectMapper, IObjectMapExpression
    {
        public object Map(ResolutionContext context)
        {
            return context.SourceValue;
        }

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsNullableType();
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return sourceExpression;
        }
    }
}