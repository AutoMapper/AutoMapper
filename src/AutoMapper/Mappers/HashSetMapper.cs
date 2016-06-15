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
        public static ISet<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source, ISet<TDestination> destination, ResolutionContext context)
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
            {
                return null;
            }

            destination = destination ?? new HashSet<TDestination>();

            destination.Clear();

            var itemContext = new ResolutionContext(context);
            foreach (var item in source ?? Enumerable.Empty<TSource>())
            {
                destination.Add(itemContext.Map(item, default(TDestination)));
            }

            return destination;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(HashSetMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            var srcType = TypeHelper.GetElementType(context.SourceType);
            var destType = TypeHelper.GetElementType(context.DestinationType);

            return MapMethodInfo.MakeGenericMethod(srcType, destType).Invoke(null, new [] { context.SourceValue, context.DestinationValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && IsSetType(context.DestinationType);

            return isMatch;
        }


        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null,
                MapMethodInfo.MakeGenericMethod(TypeHelper.GetElementType(sourceExpression.Type), TypeHelper.GetElementType(destExpression.Type)),
                    sourceExpression, destExpression, contextExpression);
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