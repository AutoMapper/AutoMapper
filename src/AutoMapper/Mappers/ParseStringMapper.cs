using System;
using System.Linq.Expressions;
namespace AutoMapper.Internal.Mappers
{
    public class ParseStringMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.SourceType == typeof(string) && HasParse(context.DestinationType);
        static bool HasParse(Type type) => type == typeof(Guid) || type == typeof(TimeSpan) || type == typeof(DateTimeOffset);
        public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            Expression.Call(destExpression.Type.GetMethod("Parse", new[] { typeof(string) }), sourceExpression);
    }
}