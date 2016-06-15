using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;

    public class HashSetMapper : IObjectMapExpression
    {
        public object Map(ResolutionContext context)
        {
            return CollectionMapper.MapBase(context, typeof(HashSet<>));
        }

        public bool IsMatch(TypePair context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && IsSetType(context.DestinationType);

            return isMatch;
        }


        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return CollectionMapper.MapExpressionBase(typeMapRegistry, configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, typeof(HashSet<>));
        }

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