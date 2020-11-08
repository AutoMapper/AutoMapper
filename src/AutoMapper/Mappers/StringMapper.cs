using AutoMapper.Internal;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using static Expression;
    public class StringMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, IMemberMap memberMap, 
            Expression sourceExpression, Expression destExpression, Expression contextExpression) => Call(sourceExpression, ExpressionFactory.ObjectToString);
    }
}