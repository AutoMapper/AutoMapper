using AutoMapper.Internal;
using System.Linq.Expressions;

namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    public class StringMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap,
            Expression sourceExpression, Expression destExpression) => Call(sourceExpression, ExpressionFactory.ObjectToString);
    }
}