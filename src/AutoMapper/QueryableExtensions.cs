using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
                _expressionCache.GetOrAdd(new TypePair(typeof(TSource), typeof(TDestination)), tp =>
                {
                    return CreateMapExpression(mappingEngine, tp.SourceType, tp.DestinationType);
                });
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

        private static LambdaExpression CreateMapExpression(IMappingEngine mappingEngine, Type typeIn, Type typeOut)
        {
            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(typeIn, "dto");

            var total = CreateMapExpression(mappingEngine, typeIn, typeOut, instanceParameter);

            return Expression.Lambda(total, instanceParameter);
        }

        private static Expression CreateMapExpression(IMappingEngine mappingEngine, Type typeIn, Type typeOut, Expression instanceParameter)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(typeIn, typeOut);

            if (typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. Create using Mapper.CreateMap<{0}, {1}>.";

                var message = string.Format(MessageFormat, typeIn.Name, typeOut.Name);

                throw new InvalidOperationException(message);
            }

            var bindings = CreateMemberBindings(mappingEngine, typeIn, typeMap, instanceParameter);

            Expression total = Expression.MemberInit(
                Expression.New(typeOut),
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

                MemberAssignment bindExpression = null;
                // next to lists, also arrays
                // and objects!!!
                if (propertyMap.DestinationPropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                    propertyMap.DestinationPropertyType != typeof(string))
                {
                    Type destinationListType = propertyMap.DestinationPropertyType.GetGenericArguments().First();
                    Type sourceListType = null;
                    // is list

                    sourceListType = result.Type.GetGenericArguments().First();

                    //var newVariableName = "t" + (i++);
                    var transformedExpression = CreateMapExpression(mappingEngine, sourceListType, destinationListType);

                    MethodCallExpression selectExpression = Expression.Call(
                        typeof (Enumerable),
                        "Select",
                        new[] {sourceListType, destinationListType},
                        result.ResolutionExpression,
                        transformedExpression);

                    var isNullExpression = Expression.Equal(result.ResolutionExpression, Expression.Constant(null, result.Type));

                    if (typeof(IList<>).MakeGenericType(destinationListType).IsAssignableFrom(propertyMap.DestinationPropertyType))
                    {
                        MethodCallExpression toListCallExpression = Expression.Call(
                            typeof (Enumerable),
                            "ToList",
                            new Type[] {destinationListType},
                            selectExpression);

                        var toListIfSourceIsNotNull =
                            Expression.Condition(
                                isNullExpression,
                                Expression.Constant(null, toListCallExpression.Type),
                                toListCallExpression);

                        // todo .ToArray()

                        bindExpression = Expression.Bind(destinationMember, toListIfSourceIsNotNull);
                    }
                    else
                    {
                        var selectIfSourceIsNotNull =
                            Expression.Condition(
                                isNullExpression,
                                Expression.Constant(null, selectExpression.Type),
                                selectExpression);

                        // destination type implements ienumerable, but is not an ilist. allow deferred enumeration
                        bindExpression = Expression.Bind(destinationMember, selectIfSourceIsNotNull);
                    }
                }
                else
                {
                    // does of course not work for subclasses etc./generic ...
                    if (result.Type != propertyMap.DestinationPropertyType &&
                        // avoid nullable etc.
                        propertyMap.DestinationPropertyType.BaseType != typeof(ValueType) &&
                        propertyMap.DestinationPropertyType.BaseType != typeof(Enum))
                    {
                        var transformedExpression = CreateMapExpression(mappingEngine, result.Type, propertyMap.DestinationPropertyType, result.ResolutionExpression);

                        var isNullExpression = Expression.Equal(result.ResolutionExpression, Expression.Constant(null));

                        var transformIfIsNotNull =
                            Expression.Condition(isNullExpression, Expression.Constant(null, propertyMap.DestinationPropertyType),
                                                 transformedExpression);

                        bindExpression = Expression.Bind(destinationMember, transformIfIsNotNull);
                    }
                    else
                    {
                        bindExpression = Expression.Bind(destinationMember, result.ResolutionExpression);
                    }
                }
                bindings.Add(bindExpression);
            }
            return bindings;
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
