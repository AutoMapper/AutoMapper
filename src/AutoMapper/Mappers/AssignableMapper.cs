using System.Linq.Expressions;
namespace AutoMapper.Internal.Mappers
{
    public class AssignableMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.DestinationType.IsAssignableFrom(context.SourceType);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression) => sourceExpression;
    }
}