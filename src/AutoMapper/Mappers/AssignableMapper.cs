using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public class AssignableMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsAssignableFrom(context.SourceType);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return sourceExpression;
        }
    }
}