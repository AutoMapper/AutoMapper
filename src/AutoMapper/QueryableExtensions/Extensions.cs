namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Impl;

    public static class Extensions
    {
        /// <summary>
        /// Create an expression tree representing a mapping from the <typeparamref name="TSource"/> type to <typeparamref name="TDestination"/> type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TDestination">Destination Type</typeparam>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Expand members explicitly previously marked as members to explicitly expand</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(
            this IMappingEngine mappingEngine, IDictionary<string, object> parameters = null,
            params string[] membersToExpand)
        {
            return
                (Expression<Func<TSource, TDestination>>)
                    mappingEngine.CreateMapExpression(typeof (TSource), typeof (TDestination), parameters,
                        membersToExpand);
        }

        /// <summary>
        /// Create an expression tree representing a mapping from the source type to destination type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <param name="sourceType">Source Type</param>
        /// <param name="destinationType">Destination Type</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Expand members explicitly previously marked as members to explicitly expand</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression CreateMapExpression(this IMappingEngine mappingEngine,
            Type sourceType, Type destinationType,
            IDictionary<string, object> parameters = null, params string[] membersToExpand)
        {
            return mappingEngine.CreateMapExpression(sourceType, destinationType, parameters, membersToExpand);
        }

        public static IQueryable<TDestination> Map<TSource, TDestination>(this IQueryable<TSource> sourceQuery,
            IQueryable<TDestination> destQuery)
        {
            return sourceQuery.Map(destQuery, Mapper.Engine);
        }

        public static IQueryable<TDestination> Map<TSource, TDestination>(this IQueryable<TSource> sourceQuery,
            IQueryable<TDestination> destQuery, IMappingEngine mappingEngine)
        {
            return QueryMapperVisitor.Map<TSource, TDestination>(sourceQuery, destQuery, mappingEngine);
        }

        public static IQueryDataSourceInjection<TSource> UseAsDataSource<TSource>(this IQueryable<TSource> dataSource)
        {
            return dataSource.UseAsDataSource(Mapper.Engine);
        }

        public static IQueryDataSourceInjection<TSource> UseAsDataSource<TSource>(this IQueryable<TSource> dataSource, IMappingEngine mappingEngine)
        {
            return new QueryDataSourceInjection<TSource>(dataSource, mappingEngine);
        }

        /// <summary>
        /// Extention method to project from a queryable using the static <see cref="Mapper.Engine"/> property.
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
            return new ProjectionExpression(source, mappingEngine);
        }

        /// <summary>
        /// Extention method to project from a queryable using the static <see cref="Mapper.Engine"/> property.
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source)
        {
            return source.ProjectTo<TDestination>(Mapper.Engine);
        }

        /// <summary>
        /// Extention method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source, IMappingEngine mappingEngine)
        {
            return new ProjectionExpression(source, mappingEngine).To<TDestination>();
        }

    }
}
