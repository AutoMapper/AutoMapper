namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Configuration;

    public class EnumerableToDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsDictionaryType()
                   && context.SourceType.IsEnumerableType()
                   && !context.SourceType.IsDictionaryType();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap,
            Expression sourceExpression, Expression destExpression, Expression contextExpression)
            =>
            configurationProvider.MapCollectionExpression(propertyMap, sourceExpression, destExpression,
                contextExpression, CollectionMapperExtensions.IfNotNull, typeof(Dictionary<,>),
                CollectionMapperExtensions.MapItemExpr);
    }
}