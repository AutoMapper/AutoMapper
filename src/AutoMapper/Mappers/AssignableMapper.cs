using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public class AssignableMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.DestinationType.IsAssignableFrom(context.SourceType);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression) 
            => sourceExpression;
    }
}