using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using static Expression;

    public class StringMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.DestinationType == typeof(string) && context.SourceType != typeof(string);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(sourceExpression, typeof(object).GetDeclaredMethod("ToString"));
        }
    }
}