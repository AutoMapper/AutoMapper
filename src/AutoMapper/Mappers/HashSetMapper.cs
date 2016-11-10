using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;

    public class HashSetMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
            => context.SourceType.IsEnumerableType() && IsSetType(context.DestinationType);

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(HashSet<>), CollectionMapperExtensions.MapItemExpr);

        private static bool IsSetType(Type type)
        {
            if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof (ISet<>))
            {
                return true;
            }

            IEnumerable<Type> genericInterfaces = type.GetTypeInfo().ImplementedInterfaces.Where(t => t.IsGenericType());
            IEnumerable<Type> baseDefinitions = genericInterfaces.Select(t => t.GetGenericTypeDefinition());

            var isCollectionType = baseDefinitions.Any(t => t == typeof (ISet<>));

            return isCollectionType;
        }
    }
}