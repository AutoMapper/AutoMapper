using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Impl;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions
{
    public static class Extensions
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();

        private static readonly Internal.IDictionary<TypePair, LambdaExpression> _expressionCache
            = DictionaryFactory.CreateDictionary<TypePair, LambdaExpression>();

        /// <summary>
        /// Create an expression tree representing a mapping from the <typeparamref name="TSource"/> type to <typeparamref name="TDestination"/> type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TDestination">Destination Type</typeparam>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(this IMappingEngine mappingEngine)
        {
            return (Expression<Func<TSource, TDestination>>)
                _expressionCache.GetOrAdd(new TypePair(typeof(TSource), typeof(TDestination)), tp => CreateMapExpression(mappingEngine, tp));
        }


        /// <summary>
        /// Extention method to project from a queryable using the static <see cref="Mapper.Engine"/> property
        /// Due to generic parameter inference, you need to call Project().To to execute the map
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <returns>Expression to project into</returns>
        public static IProjectionExpression Project<TSource>(
            this IQueryable<TSource> source)
        {
            return source.Project(Mapper.Engine);
        }

        /// <summary>
        /// Extention method to project from a queryable using the provided mapping engine
        /// Due to generic parameter inference, you need to call Project().To to execute the map
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <returns>Expression to project into</returns>
        public static IProjectionExpression Project<TSource>(
            this IQueryable<TSource> source, IMappingEngine mappingEngine)
        {
            return new ProjectionExpression<TSource>(source, mappingEngine);
        }

        private static LambdaExpression CreateMapExpression(IMappingEngine mappingEngine, TypePair typePair)
        {
            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(typePair.SourceType, "dto");

            var total = CreateMapExpression(mappingEngine, typePair, instanceParameter);

            return Expression.Lambda(total, instanceParameter);
        }

        private static Expression CreateMapExpression(IMappingEngine mappingEngine, TypePair typePair, Expression instanceParameter)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(typePair.SourceType, typePair.DestinationType);

            if (typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. Create using Mapper.CreateMap<{0}, {1}>.";

                var message = string.Format(MessageFormat, typePair.SourceType.Name, typePair.DestinationType.Name);

                throw new InvalidOperationException(message);
            }

            var bindings = CreateMemberBindings(mappingEngine, typePair.SourceType, typeMap, instanceParameter);

            Expression total = Expression.MemberInit(
                Expression.New(typePair.DestinationType),
                bindings.ToArray()
                );

            return total;
        }

        private static List<MemberBinding> CreateMemberBindings(IMappingEngine mappingEngine, Type typeIn, TypeMap typeMap,
                                                 Expression instanceParameter)
        {
            var bindings = new List<MemberBinding>();
            foreach (var propertyMap in typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var result = propertyMap.ResolveExpression(typeIn, instanceParameter);

                var destinationMember = propertyMap.DestinationProperty.MemberInfo;

                MemberAssignment bindExpression;

                if (propertyMap.DestinationPropertyType.IsAssignableFrom(result.Type))
                {
                    bindExpression = Expression.Bind(destinationMember, result.ResolutionExpression);
                }
                else if (propertyMap.DestinationPropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                    propertyMap.DestinationPropertyType != typeof(string))
                {
                    Type destinationListType = GetDestinationListTypeFor(propertyMap);
                    Type sourceListType = null;
                    // is list

                    sourceListType = result.Type.GetGenericArguments().First();
                    var listTypePair = new TypePair(sourceListType, destinationListType);

                    //var newVariableName = "t" + (i++);
                    var transformedExpression = CreateMapExpression(mappingEngine, listTypePair);

                    MethodCallExpression selectExpression = Expression.Call(
                        typeof(Enumerable),
                        "Select",
                        new[] { sourceListType, destinationListType },
                        result.ResolutionExpression,
                        transformedExpression);

                    if (typeof(IList<>).MakeGenericType(destinationListType).IsAssignableFrom(propertyMap.DestinationPropertyType))
                    {
                        var toListCallExpression = GetToListCallExpression(propertyMap, destinationListType, selectExpression);
                        bindExpression = Expression.Bind(destinationMember, toListCallExpression);
                    }
                    else if (typeof(ICollection<>).MakeGenericType(destinationListType).IsAssignableFrom(propertyMap.DestinationPropertyType))
                    {
                        var toListCallExpression = GetToListCallExpression(propertyMap, destinationListType, selectExpression);
                        bindExpression = Expression.Bind(destinationMember, toListCallExpression);
                    }
                    else
                    {
                        // destination type implements ienumerable, but is not an ilist. allow deferred enumeration
                        bindExpression = Expression.Bind(destinationMember, selectExpression);
                    }
                }
                else if (result.Type != propertyMap.DestinationPropertyType &&
                    // avoid nullable etc.
                         propertyMap.DestinationPropertyType.BaseType != typeof(ValueType) &&
                         propertyMap.DestinationPropertyType.BaseType != typeof(Enum))
                {
                    var transformedExpression = CreateMapExpression(mappingEngine, new TypePair(result.Type, propertyMap.DestinationPropertyType),
                                                                    result.ResolutionExpression);

                    bindExpression = Expression.Bind(destinationMember, transformedExpression);
                }
                else
                {
                    throw new AutoMapperMappingException("Unable to create a map expression from " + result.Type + " to " + propertyMap.DestinationPropertyType);
                }

                bindings.Add(bindExpression);
            }
            return bindings;
        }

        private static Type GetDestinationListTypeFor(PropertyMap propertyMap)
        {
            Type destinationListType;
            if (propertyMap.DestinationPropertyType.IsArray)
                destinationListType = propertyMap.DestinationPropertyType.GetElementType();
            else
                destinationListType = propertyMap.DestinationPropertyType.GetGenericArguments().First();
            return destinationListType;
        }

        private static MethodCallExpression GetToListCallExpression(PropertyMap propertyMap, Type destinationListType, MethodCallExpression selectExpression)
        {
            return Expression.Call(
                typeof(Enumerable),
                propertyMap.DestinationPropertyType.IsArray ? "ToArray" : "ToList",
                new Type[] { destinationListType },
                selectExpression);
        }
    }

    /// <summary>
    /// Continuation to execute projection
    /// </summary>
    public interface IProjectionExpression
    {
        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <typeparam name="TResult">Destination type to map to</typeparam>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        IQueryable<TResult> To<TResult>();
    }

    public class ProjectionExpression<TSource> : IProjectionExpression
    {
        private readonly IQueryable<TSource> _source;
        private readonly IMappingEngine _mappingEngine;

        public ProjectionExpression(IQueryable<TSource> source, IMappingEngine mappingEngine)
        {
            _source = source;
            _mappingEngine = mappingEngine;
        }

        public IQueryable<TResult> To<TResult>()
        {
            Expression<Func<TSource, TResult>> expr = _mappingEngine.CreateMapExpression<TSource, TResult>();

            return _source.Select(expr);
        }
    }
}
