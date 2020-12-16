using AutoMapper.Internal;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public class AssignableMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.DestinationType.IsAssignableFrom(context.SourceType) ||
            context.IsEnumToUnderlyingType() || context.IsUnderlyingTypeToEnum();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
            => sourceExpression;
    }
}