using AutoMapper.Internal;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public class AssignableMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.DestinationType.IsAssignableFrom(context.SourceType);

        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression) 
            => sourceExpression;
    }
}